using Services.ChatBot.DTOs;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Services.ChatBot.Interfaces;
using Shared.Core.Entities;
using Telegram.Bot.Types.ReplyMarkups;
using Shared.Core.Data;
using Microsoft.EntityFrameworkCore;
using Services.ChatBot.Utils;

namespace Webhook.Controllers.Services;

public class UpdateHandler(ITelegramBotClient bot,
ILogger<UpdateHandler> logger,
IHttpClientFactory httpClientFactory,
IMenuUI menuUI,
ICatalogoUI catalogoUI,
IUtilsUI utilsUI,
IBotPersistencia _persistencia,
ApplicationDbContext context,
BotRenderer renderer,
BotInteractionHandler interactionHandler
) : IUpdateHandler
{
    private static readonly InputPollOption[] PollOptions = ["Hello", "World!"];
    private readonly HttpClient _gateway = httpClientFactory.CreateClient("GatewayApi");
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

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
            var tiempoLimite = TimeSpan.FromSeconds(120);
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
    async Task<Message> RemoveKeyboard(Message msg)
    {
        return await bot.EditMessageText(msg.Chat, msg.Id, "Removing keyboard", replyMarkup: null);
    }

    private async Task OnMessage(Message msg, string text)
    {
        var conv = await _persistencia.ObtenerConversacionActiva(msg.From.Id);
        var lastMsg = await context.Mensajes.Where(m =>
        m.ConversacionId == conv.Id)
        .OrderByDescending(m => m.FechaEnvio)
        .FirstOrDefaultAsync();
        Console.WriteLine("Contenido" + lastMsg.Contenido + " ");
        if (text == "/start" || text.ToLower().Contains("Catalogo"))
        {
            Console.WriteLine("Punto A");
            CallbackQuery callbackQuerry = new()
            {
                Data = "menu",
                Message = new Message
                {
                    Chat = msg.Chat
                }
            };
            await renderer.RenderizarMenu(bot, msg, callbackQuerry);
            return;
        }
        if (conv == null) return;
        if (lastMsg != null && lastMsg.Remitente == TipoRemitente.Sistema && lastMsg.Contenido.Contains("[ID:"))
        {
            if (int.TryParse(text, out int cantidad) && cantidad > 0)
            {
                string fragmento = lastMsg.Contenido.Split('[', ']')[1]; // "ID:2_3_0"
                string[] partes = fragmento.Split(':')[1].Split('_');    // ["2", "3", "0"]

                int prodId = int.Parse(partes[0]);
                int catId = int.Parse(partes[1]);
                int page = int.Parse(partes[2]);
                Console.WriteLine($"prodId {prodId}, catId {catId}, page {page}");

                string data = (catId == -1) ? "cart" : $"prod_{prodId}_{catId}_{page}";

                CallbackQuery callbackQuery = new()
                {
                    Data = data,
                    Message = new Message
                    {
                        Chat = msg.Chat,
                    }
                };

                Console.Error.WriteLine($"\nId: {callbackQuery.Message.MessageId}, conversacion Asunt: {conv.Asunto}\n");
                await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);
                if (catId == -1)
                {
                    await renderer.RenderizarCarrito(bot, callbackQuery, int.Parse(conv.Asunto!));
                    return;
                }
                else
                {
                    await renderer.RenderizarProducto(bot, prodId, catId, page, callbackQuery, int.Parse(conv.Asunto!), cantidad);
                    return;
                }
            }
            else
            {
                await bot.SendMessage(msg.Chat.Id, "Valor invalido. Por favor, solo numeros mayores a 0.");
                return;
            }
        }

        if (text == "/remove")
        {
            await RemoveKeyboard(msg);
        }
        await bot.SendMessage(msg.Chat, "Usa /start para ver el catalogo");
    }
    private async Task OnCallbackQuery(CallbackQuery callbackQuerry)
    {
        //Logica de consumo de productos
        var rf = callbackQuerry.Data;
        if (string.IsNullOrEmpty(rf)) return;

        var parts = rf.Split('_');
        var action = parts[0];
        Console.WriteLine($"Chat {callbackQuerry.Message!.Chat}, MessageID {callbackQuerry.Message.MessageId}");
        Console.WriteLine(action);
        Console.WriteLine(parts.Length + " line parts " + rf.ToString());

        if (action == "pcat")
        {
            int page = int.Parse(parts[1]);
            await renderer.RenderizarCategorias(bot, page, callbackQuerry);
        }
        if (action == "cat" || action == "pprod")
        {
            int catId = int.Parse(parts[1]);
            int page = parts.Length > 2 ? int.Parse(parts[2]) : 0;
            await renderer.RenderizarCatalogo(bot, callbackQuerry, catId, page);
        }

        if (action == "menu")
        {
            await renderer.RenderizarMenu(bot, callbackQuerry.Message!, callbackQuerry);
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
        {
            await interactionHandler.ManejarCambioCantidad(bot, parts, callbackQuerry, action);
        }

        if (rf.StartsWith("edit_qty_"))
        {
            await interactionHandler.ManejarEdicionManual(bot, parts, callbackQuerry);
        }

        if (rf.StartsWith("add_prod_"))
        {
            await interactionHandler.ManejarAgregarAlCarrito(bot, parts, callbackQuerry);
        }

        if (action == "cart")
        {
            await renderer.RenderizarCarrito(bot, callbackQuerry, callbackQuerry.Message!.MessageId);
        }
        if (rf.StartsWith("ask_rmv"))
        {
            await interactionHandler.ManejarAskEliminarItem(bot, callbackQuerry, parts);
        }
        if (rf.StartsWith("ask_clear"))
        {
            await interactionHandler.ManejarAskVaciarCarrito(bot, callbackQuerry);
        }
        if (action == "clear")
        {
            await interactionHandler.ManejarVaciarCarrito(bot, callbackQuerry);
        }
        if (rf.StartsWith("upd_prod_"))
        {
            await interactionHandler.ManejarEditarItem(bot, parts, callbackQuerry);
        }
        if (rf.StartsWith("rmv"))
        {
            await interactionHandler.ManejarEliminarItem(bot, parts, callbackQuerry);
        }
    }
    /*private async Task RenderizarCatalogo(ITelegramBotClient bot, CallbackQuery callbackQuerry, int catId, int page)
    {
        Console.WriteLine($"CatId {callbackQuerry.Data}, Page {page}");
        var data = await _gateway.GetFromJsonAsync<PagedResult<ProductoDTO>>($"productos/list-4/{catId}?page={page}&pageSize=4");
        var categoria = await _gateway.GetFromJsonAsync<CategoriaDTO>($"categorias/{catId}");
        var markup = catalogoUI.BuildUIProductos(data, catId, page);
        if (data == null || !data.Items.Any())
        {
            await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, $" {categoria.Nombre}\n No se encontraron Productos:", replyMarkup: markup);
        }
        else
        {
            // Usamos la interfaz de productos                
            await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, $" {categoria.Nombre}\n 🛍 Productos:", replyMarkup: markup);
        }
    }

    private async Task RenderizarMenu(ITelegramBotClient bot, Message msg, CallbackQuery callbackQuery)
    {
        var telegramId = msg.From!.Id;

        var name = $"{msg.From.FirstName} {msg.From.LastName}".Trim();
        await _persistencia.RegistrarCliente(telegramId, name);

        string tiendaName = "tienda";
        string welcomeText = $"👋 Hola {name}, bienvenido a nuestra {tiendaName}. ";
        Message send;
        var markup = menuUI.BuildUIHome(name);

        if (callbackQuery.Message!.MessageId == 0)
        {
            Console.WriteLine("\nNuevo mensaje para mostrar menu\n");
            send = await bot.SendMessage(msg.Chat, welcomeText, parseMode: ParseMode.Markdown, replyMarkup: markup);
        }
        else
        {
            Console.WriteLine("\nEditando mensaje para mostrar menu\n");
            send = await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId,
                welcomeText, parseMode: ParseMode.Markdown, replyMarkup: markup);
        }
        await _persistencia.ActualizarConversacion(msg.From.Id, send.Id, true);
    }

    private async Task RenderizarCategorias(ITelegramBotClient bot, int page, CallbackQuery callbackQuery)
    {
        var data = await _gateway.GetFromJsonAsync<PagedResult<CategoriaDTO>>($"categorias/list-6?page={page}&pageSize=6");
        if (data == null || !data.Items.Any()) return;
        // Usamos la interfaz de categorías
        var markup = catalogoUI.BuildUICategorias(data, page);
        await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, "📂 Menú:", replyMarkup: markup);
    }

    private async Task RenderizarProducto(ITelegramBotClient bot, int prodId, int catId, int page, CallbackQuery callbackQuery, int cantidad)
    {
        var data = await _gateway.GetFromJsonAsync<ProductoDTO>($"productos/{prodId}");
        if (data == null) return;

        string msg = $"📦 {data.Nombre}\n" +
        $"\n\tPrecio: ${data.Precio}" +
        $"\n\tStock: {data.StockDisponible}";

        var keyboard = catalogoUI.BuildUIDetalleProducto(prodId, catId, page, cantidad);

        await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, msg, replyMarkup: keyboard);

        Console.WriteLine($"name {data.Nombre}, precio {data.Precio}, stock {data.StockDisponible}");
    }

    private async Task ManejarCambioCantidad(ITelegramBotClient bot, string[] parts, CallbackQuery callbackQuery, string action)
    {
        int prodId = int.Parse(parts[1]);
        int catId = int.Parse(parts[2]);
        int page = int.Parse(parts[3]);
        var currentMkp = callbackQuery.Message!.ReplyMarkup;

        int currentQty = int.Parse(currentMkp.InlineKeyboard.ElementAt(0).ElementAt(1).Text);

        if (action == "inc") currentQty++;
        else if (currentQty > 1) currentQty--;

        var keyboard = catalogoUI.BuildUIDetalleProducto(prodId, catId, page, currentQty);

        await bot.EditMessageReplyMarkup(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, keyboard);
    }

    private async Task ManejarEdicionManual(ITelegramBotClient bot, string[] parts, CallbackQuery callbackQuery)
    {
        int prodId = int.Parse(parts[2]);
        int catId = int.Parse(parts[3]);
        int page = int.Parse(parts[4]);

        string instruction = $"*Ingreso Manual*\n\nEscribe la cantidad que deseas para el producto [ID:{prodId}]";
        await bot.AnswerCallbackQuery(callbackQuery.Id, "⌨️ Escribe la cantidad en el chat", showAlert: false);
        Console.WriteLine(instruction);

        var cancelKbd = new InlineKeyboardMarkup(
            InlineKeyboardButton.WithCallbackData("Cancelar", $"prod_{prodId}_{catId}_{page}")
        );

        await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, instruction, parseMode: ParseMode.Markdown, replyMarkup: cancelKbd);

        var conv = await _persistencia.ObtenerConversacionActiva(callbackQuery.From.Id);
        if (conv != null)
        {
            await _persistencia.RegistrarMensaje(conv.Id, $" [ID:{prodId}_{catId}_{page}] Esperando cantidad manual...", TipoRemitente.Sistema);
        }
    }

    public async Task ManejarAgregarAlCarrito(ITelegramBotClient bot, string[] parts, CallbackQuery callbackQuery)
    {
        int prodId = int.Parse(parts[2]);
        int cantidad = int.Parse(parts[3]);
        int catId = int.Parse(parts[4]);
        int page = int.Parse(parts[5]);
        if (cantidad > 0)
        {
            var resultado = await _persistencia.AgregarProducto(callbackQuery.From.Id, prodId, cantidad);
            if (resultado.Success)
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, resultado.msg);
                await RenderizarCatalogo(bot, callbackQuery, catId, page);
            }
            else
                await bot.AnswerCallbackQuery(callbackQuery.Id, $"Error: {resultado.msg}", showAlert: true);
        }
        else
            await bot.AnswerCallbackQuery(callbackQuery.Id, $"Error: cantidad invalida", showAlert: true);
    }*/
}
