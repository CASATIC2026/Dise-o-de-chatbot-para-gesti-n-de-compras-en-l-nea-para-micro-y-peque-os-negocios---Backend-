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
    private readonly string url = "https://placehold.co/360x100/png?text=Tienda";
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
        //return await bot.EditMessageText(msg.Chat, msg.Id, "Removing keyboard", replyMarkup: null);
        return await bot.EditMessageCaption(msg.Chat, msg.Id, "Removing keyboard", replyMarkup: null);
    }

    private async Task OnMessage(Message msg, string text)
    {
        var conv = await _persistencia.ObtenerConversacionActiva(msg.From.Id);

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
        var lastMsg = await context.Mensajes.Where(m =>
        m.ConversacionId == conv.Id)
        .OrderByDescending(m => m.FechaEnvio)
        .FirstOrDefaultAsync();

        if (lastMsg != null)
            Console.WriteLine("Contenido: " + lastMsg!.Contenido + " ");

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
                //await bot.SendMessage(msg.Chat.Id, "Valor invalido. Por favor, solo numeros mayores a 0.");
                await bot.SendPhoto(msg.Chat.Id, url, "Valor invalido. Por favor, solo numeros mayores a 0.", parseMode: ParseMode.Markdown);
                return;
            }
        }

        if (lastMsg != null && lastMsg.Contenido.Contains("[ESTADO:CHECKOUT_DIRECCION]") && lastMsg.Remitente == TipoRemitente.Sistema)
        {
            string direccion = text;

            await _persistencia.ActualizarPedido(msg.From.Id, new PedidoDTO
            {
                Estado = EstadoPedido.Pendiente,
                Direccion = direccion
            });

            await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);

            string instruction = "📍 PASO 2 :REFERENCIAS DE UBICACION\n\nPor favor, escribe referencias como: \n'frente a la tienda X' o 'casa color verde'";
            await _persistencia.RegistrarMensaje(conv.Id, $"[ESTADO:CHECKOUT_REFERENCIAS]_[{conv.Asunto!}]_*Esperando referencias...", TipoRemitente.Sistema);

            //await bot.EditMessageText(msg.Chat.Id, int.Parse(conv.Asunto!), instruction, parseMode: ParseMode.Markdown);
            await bot.EditMessageCaption(msg.Chat.Id, int.Parse(conv.Asunto!), instruction, parseMode: ParseMode.Markdown);
            return;
        }
        if (lastMsg != null && lastMsg.Contenido.Contains("[ESTADO:CHECKOUT_REFERENCIAS]") && lastMsg.Remitente == TipoRemitente.Sistema)
        {
            string referencias = text;
            string Asunto = lastMsg.Contenido.Split('_')[2].Trim('[', ']');

            await _persistencia.ActualizarPedido(msg.From.Id, new PedidoDTO
            {
                Detalles = new PedidoDetalleDTO { Referencias = referencias }
            });
            await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);

            string instruction = "📞 PASO 3: TELÉFONO DE CONTACTO\n\nEscribe tu número de teléfono para coordinar la entrega:";
            await _persistencia.RegistrarMensaje(conv.Id, $"[ESTADO:CHECKOUT_TELEFONO]_[{Asunto}]_Esperando teléfono...", TipoRemitente.Sistema);

            //await bot.EditMessageText(msg.Chat.Id, int.Parse(Asunto!), instruction, parseMode: ParseMode.Markdown);
            await bot.EditMessageCaption(msg.Chat.Id, int.Parse(Asunto!), instruction, parseMode: ParseMode.Markdown);
            return;
        }

        if (lastMsg != null && lastMsg.Contenido.Contains("[ESTADO:CHECKOUT_TELEFONO]"))
        {
            string telefono = text;

            string Asunto = lastMsg.Contenido.Split('_')[2].Trim('[', ']'); // Dependiendo de tu formato exacto
            Console.WriteLine($"\nAsunto: {Asunto}\n");

            await _persistencia.ActualizarPedido(msg.From.Id, new PedidoDTO
            {
                Detalles = new PedidoDetalleDTO { Telefono = telefono }
            });

            await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);

            CallbackQuery callbackQuery = new()
            {
                Data = "menu",
                From = msg.From,
                Message = new Message
                {
                    Chat = msg.Chat,
                }
            };
            await renderer.RenderizarResumenFina(bot, callbackQuery, int.Parse(Asunto!));

            // 4. Limpiamos el estado
            await _persistencia.RegistrarMensaje(conv.Id, "[ESTADO:REPOSO]", TipoRemitente.Sistema);
            return;
        }


        if (text == "/remove")
        {
            await RemoveKeyboard(msg);
            return;
        }
        await bot.SendMessage(msg.Chat, "Usa /start para ver el catalogo");
        //await bot.SendPhoto(msg.Chat, url, "Usa /start para ver el catalogo", parseMode: ParseMode.Markdown);
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
        if (action == "checkout")
        {
            var pedido = await _persistencia.ObtenerPedidoActivo(callbackQuerry.From.Id);
            if (pedido == null || !pedido.PedidoProductos.Any())
            {
                await bot.AnswerCallbackQuery(callbackQuerry.Id, "⚠️ Tu carrito está vacío.", showAlert: true);
                return;
            }
            var conv = await _persistencia.ObtenerConversacionActiva(callbackQuerry.From.Id);

            string instruction = "📍 PASO 1: DIRECCIÓN DE ENVÍO\n\nPor favor, escribe tu dirección exacta";
            await _persistencia.RegistrarMensaje(conv!.Id, $" [ESTADO:CHECKOUT_DIRECCION]_Esperando dirección..._", TipoRemitente.Sistema);
            //await bot.EditMessageText(callbackQuerry.Message!.Chat.Id, callbackQuerry.Message.MessageId, instruction, parseMode: ParseMode.Markdown);
            await bot.EditMessageCaption(callbackQuerry.Message!.Chat.Id, callbackQuerry.Message.MessageId, instruction, parseMode: ParseMode.Markdown);
            await bot.AnswerCallbackQuery(callbackQuerry.Id);
        }

        if (action == "ords")
        {
            await renderer.RenderizarOrdenes(bot, callbackQuerry, 0);
        }
        if (action == "pords")
        {
            int page = int.Parse(parts[1]);
            await renderer.RenderizarOrdenes(bot, callbackQuerry, page);
        }

        if (action == "checkoutEnd")
        {
            await interactionHandler.ManejarFinalizacionPedido(bot, callbackQuerry);
        }
    }
}
