using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Services.ChatBot.Interfaces;
using Shared.Core.Entities;

namespace Webhook.Controllers.Services;

/// <summary>
/// Handles Telegram bot updates including messages and callback queries.
/// Processes user interactions such as catalog browsing, product selection, cart management, and checkout workflow.
/// </summary>
public class UpdateHandler(ITelegramBotClient bot,
ILogger<UpdateHandler> logger,
IHttpClientFactory httpClientFactory,
IUtilsUI utilsUI,
IBotPersistencia _persistencia,
BotRenderer renderer,
BotInteractionHandler interactionHandler,
BotOnMsgInteractionHandler onMsgInteractionHandler
) : IUpdateHandler
{
    private readonly HttpClient _gateway = httpClientFactory.CreateClient("GatewayApi");
    private readonly string url = "https://placehold.co/360x100/png?text=Tienda";

    /// <summary>
    /// Handles errors that occur during bot update processing.
    /// Implements a cooldown delay for network connection errors.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="source">The source of the error.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    /// <summary>
    /// Processes incoming Telegram updates including messages and callback queries.
    /// Validates callback queries against active conversation session tokens and enforces timeout limits.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="update">The incoming Telegram update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Text: { } } msg)
        {
            await OnMessage(msg, msg.Text);
            return;
        }

        if (update.CallbackQuery is not { } cb) return;
        var conv = await _persistencia.ObtenerConversacionActiva(cb.From.Id);
        if (conv != null)
        {
            var tiempoLimite = TimeSpan.FromSeconds(300); //Limited de token de conversacion
            var inactividad = DateTime.UtcNow - conv.ActualizadoEn;

            bool esMessajeValido = cb.Message!.MessageId.ToString() == conv.Asunto;
            bool estaEnTiempo = inactividad < tiempoLimite;

            if (!esMessajeValido || !estaEnTiempo)
            {
                await bot.AnswerCallbackQuery(cb.Id, "❌ Sesión expirada", showAlert: true);
                await utilsUI.InvalidarMenu(cb.Message.Chat.Id, cb.Message.MessageId, "expirado", null);
                return;
            }
            await _persistencia.RegistrarMensaje(conv.Id, $"Clic en: {cb.Data}", TipoRemitente.Cliente);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { Text: { } text } message } => OnMessage(message, text),
            { CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery),
            _ => Task.CompletedTask
        });
    }
    /// <summary>
    /// Removes the inline keyboard from a message by editing its caption.
    /// </summary>
    /// <param name="msg">The message to remove the keyboard from.</param>
    /// <returns>The updated message.</returns>
    async Task<Message> RemoveKeyboard(Message msg)
    {
        //return await bot.EditMessageText(msg.Chat, msg.Id, "Removing keyboard", replyMarkup: null);
        return await bot.EditMessageCaption(msg.Chat, msg.Id, "Removing keyboard", replyMarkup: null);
    }

    /// <summary>
    /// Processes text messages from users, routing them to appropriate handlers based on content.
    /// Supports start command, catalog browsing, manual product quantity editing, and checkout workflow.
    /// </summary>
    /// <param name="msg">The message object containing sender and chat information.</param>
    /// <param name="text">The text content of the message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OnMessage(Message msg, string text)
    {
        if (text == "/start" || text.ToLower().Contains("Catalogo"))
        {
            await onMsgInteractionHandler.ManejoMsgInicioConversacion(bot, msg);
            return;
        }
        var conv = await _persistencia.ObtenerConversacionActiva(msg.From!.Id);
        if (conv == null) return;

        if (await onMsgInteractionHandler.ManejoMsgEdicionManualCantProduc(bot, msg, text)) return;

        if (await onMsgInteractionHandler.ManejoMsgCheckout(bot, msg, text)) return;

        if (text == "/remove")
        {
            await RemoveKeyboard(msg);
            return;
        }
        await bot.SendMessage(msg.Chat, "Usa /start para ver el catalogo");
    }

    /// <summary>
    /// Processes callback queries from inline buttons, routing to specific handlers based on the action type.
    /// Supports product catalog navigation, cart operations, and checkout workflow actions.
    /// </summary>
    /// <param name="callbackQuerry">The callback query from the user's button interaction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OnCallbackQuery(CallbackQuery callbackQuerry)
    {
        //Logica de consumo de productos
        var rf = callbackQuerry.Data;
        if (string.IsNullOrEmpty(rf)) return;

        var parts = rf.Split('_');
        var action = parts[0];
        //Console.WriteLine($"Chat {callbackQuerry.Message!.Chat}, MessageID {callbackQuerry.Message.MessageId}");
        //Console.WriteLine(action);
        //Console.WriteLine(parts.Length + " line parts " + rf.ToString());

        if (action == "pcat")
        {
            int page = int.Parse(parts[1]);
            await renderer.RenderizarCategorias(bot, page, callbackQuerry);
            return;
        }
        if (action == "cat" || action == "pprod")
        {
            int catId = int.Parse(parts[1]);
            int page = parts.Length > 2 ? int.Parse(parts[2]) : 0;
            await renderer.RenderizarCatalogo(bot, callbackQuerry, catId, page);
            return;
        }
        if (action == "menu")
        {
            await renderer.RenderizarMenu(bot, callbackQuerry.Message!, callbackQuerry);
            return;
        }
        if (action == "prod")
        {
            int prodId = int.Parse(parts[1]);
            int catId = int.Parse(parts[2]);
            int page = int.Parse(parts[3]);
            int cantidad = (parts.Length > 4) ? int.Parse(parts[4]) : 0;
            await renderer.RenderizarProducto(bot, prodId, catId, page, callbackQuerry, callbackQuerry.Message!.MessageId, cantidad);
        }

        if (action == "inc" || action == "dec")
            await interactionHandler.ManejarCambioCantidad(bot, parts, callbackQuerry, action);

        if (rf.StartsWith("edit_qty_"))
            await interactionHandler.ManejarEdicionManual(bot, parts, callbackQuerry);

        if (rf.StartsWith("add_prod_"))
            await interactionHandler.ManejarAgregarAlCarrito(bot, parts, callbackQuerry);

        if (action == "cart")
            await renderer.RenderizarCarrito(bot, callbackQuerry, callbackQuerry.Message!.MessageId);

        if (rf.StartsWith("ask_rmv"))
            await interactionHandler.ManejarAskEliminarItem(bot, callbackQuerry, parts);

        if (rf.StartsWith("ask_clear"))
            await interactionHandler.ManejarAskVaciarCarrito(bot, callbackQuerry);

        if (action == "clear")
            await interactionHandler.ManejarVaciarCarrito(bot, callbackQuerry);

        if (rf.StartsWith("upd_prod_"))
            await interactionHandler.ManejarEditarItem(bot, parts, callbackQuerry);

        if (rf.StartsWith("rmv"))
            await interactionHandler.ManejarEliminarItem(bot, parts, callbackQuerry);

        if (action == "checkout")
            await interactionHandler.ManejarRegistroDireccionEnvio(bot, callbackQuerry);

        if (action == "ords")
            await renderer.RenderizarOrdenes(bot, callbackQuerry, 0);

        if (action == "pords")
        {
            int page = int.Parse(parts[1]);
            await renderer.RenderizarOrdenes(bot, callbackQuerry, page);
        }
        if (action == "checkoutEnd")
            await interactionHandler.ManejarFinalizacionPedido(bot, callbackQuerry);
    }
}
