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


namespace Webhook.Controllers.Services;

public class UpdateHandler(ITelegramBotClient bot,
ILogger<UpdateHandler> logger,
IHttpClientFactory httpClientFactory,
IMenuUI menuUI,
ICatalogoUI catalogoUI,
IUtilsUI utilsUI,
IBotPersistencia _persistencia,
ApplicationDbContext context
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
                await utilsUI.InvalidarMenu(cb.Message.Chat.Id, cb.Message.MessageId, "expierado", null);
                await utilsUI.InvalidarMenu(cb.Message.Chat.Id, cb.Message.MessageId, "expierado", null);
                return;
            }
        }
        if (conv != null) await _persistencia.RegistrarMensaje(conv.Id, $"Clic en: {cb.Data}", TipoRemitente.Cliente);

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

        if (text == "/start" || text.ToLower().Contains("Catalogo"))
        {

            var telegramId = msg.From.Id;
            var name = msg.From.FirstName + "" + msg.From.LastName;
            await _persistencia.RegistrarCliente(telegramId, name.Trim());
            var data = await _gateway.GetFromJsonAsync<PagedResult<CategoriaDTO>>("categorias/list-6?page=0&pageSize=6");
            var markup = menuUI.BuildUICategorias(data, 0);
            Console.WriteLine("Punto A");
            CallbackQuery callbackQuerry = new CallbackQuery
            {
                Data = "pcat_0",
                Message = new Message
                {
                    Chat = msg.Chat
                }
            };
            Console.WriteLine("Punto B " + msg.Id);
            Console.WriteLine("Punto B " + msg.Id);

            // 3. Enviar el menú
            var enviado = await bot.SendMessage(msg.Chat, "📂 *Bienvenido al Catálogo*\nSelecciona una categoría:",
                parseMode: ParseMode.Markdown,
                replyMarkup: markup);
            Console.WriteLine("id msg" + enviado.Id);
            await _persistencia.ActualizarConversacion(msg.From.Id, enviado.Id, true);

            if (conv != null)
            {
                await _persistencia.RegistrarMensaje(conv.Id, "Comando /start ejecutado", TipoRemitente.Cliente);
            }
            return;
        }
        if (conv == null) return;

        var lastMsg = await context.Mensajes.Where(m =>
        m.ConversacionId == conv.Id)
        .OrderByDescending(m => m.FechaEnvio)
        .FirstOrDefaultAsync();
        Console.WriteLine("Conteido" + lastMsg.Contenido + " ");
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

                var data = await _gateway.GetFromJsonAsync<ProductoDTO>($"productos/{prodId}");

                await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);
                if (data != null)
                {
                    string fichaMsg = $"📦 {data.Nombre}\n" +
                        $"\n\tPrecio: ${data.Precio}" +
                        $"\n\tStock: {data.StockDisponible}";

                    var keyboard = catalogoUI.BuildUIDetalleProducto(prodId, catId, page, cantidad);

                    await bot.EditMessageText(msg.Chat.Id, int.Parse(conv.Asunto), fichaMsg, parseMode: ParseMode.Markdown, replyMarkup: keyboard);
                }
                return;
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
            var data = await _gateway.GetFromJsonAsync<PagedResult<CategoriaDTO>>($"categorias/list-6?page={page}&pageSize=6");
            if (data == null || !data.Items.Any()) return;
            // Usamos la interfaz de categorías
            var markup = menuUI.BuildUICategorias(data, page);
            //Console.WriteLine($"Chat {callbackQuerry.Message!.Chat}, MessageID {callbackQuerry.Message.MessageId}, Markup {data.TotalCount}");
            if (callbackQuerry.Message.MessageId == 0)
                await bot.SendMessage(callbackQuerry.Message!.Chat, "📂 Menú:", replyMarkup: markup);
            else
                await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, "📂 Menú:", replyMarkup: markup);
        }

        else if (action == "cat" || action == "pprod")
        {
            int catId = int.Parse(parts[1]);
            int page = parts.Length > 2 ? int.Parse(parts[2]) : 0;
            await RenderizarCatalogo(bot, callbackQuerry, catId, page);
        }

        if (action == "prod")
        {

            int prodId = int.Parse(parts[1]);
            int catId = int.Parse(parts[2]);
            int page = int.Parse(parts[3]);

            var data = await _gateway.GetFromJsonAsync<ProductoDTO>($"productos/{prodId}");

            string msg = $"📦 {data.Nombre}\n" +
            $"\n\tPrecio: ${data.Precio}" +
            $"\n\tStock: {data.StockDisponible}";

            var keyboard = catalogoUI.BuildUIDetalleProducto(prodId, catId, page, 0);

            await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, msg, replyMarkup: keyboard);

            Console.WriteLine($"name {data.Nombre}, precio {data.Precio}, stock {data.StockDisponible}");
            //await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, $"name {data.Nombre}, precio {data.Precio}, stock {data.StockDisponible}", replyMarkup: null);
            //await utilsUI.InvalidarMenu(callbackQuerry.Message.Chat.Id, callbackQuerry.Message.MessageId, "Selección procesada.", action);
        }

        if (action == "inc" || action == "dec")
        {
            int prodId = int.Parse(parts[1]);
            int catId = int.Parse(parts[2]);
            int page = int.Parse(parts[3]);
            var currentMkp = callbackQuerry.Message!.ReplyMarkup;

            int currentQty = int.Parse(currentMkp.InlineKeyboard.ElementAt(0).ElementAt(1).Text);

            if (action == "inc") currentQty++;
            else if (currentQty > 1) currentQty--;

            var keyboard = catalogoUI.BuildUIDetalleProducto(prodId, catId, page, currentQty);

            await bot.EditMessageReplyMarkup(callbackQuerry.Message.Chat.Id, callbackQuerry.Message.MessageId, keyboard);
        }
        
        if (rf.StartsWith("edit_qty_"))
        {
            int prodId = int.Parse(parts[2]);
            int catId = int.Parse(parts[3]);
            int page = int.Parse(parts[4]);

            string instruction = $"*Ingreso Manual*\n\nEscribe la cantidad que deseas para el producto [ID:{prodId}]";
            await bot.AnswerCallbackQuery(callbackQuerry.Id, "⌨️ Escribe la cantidad en el chat", showAlert: false);
            Console.WriteLine(instruction);

            var cancelKbd = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("Cancelar", $"prod_{prodId}_{catId}_{page}")
            );

            await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, instruction, parseMode: ParseMode.Markdown, replyMarkup: cancelKbd);

            var conv = await _persistencia.ObtenerConversacionActiva(callbackQuerry.From.Id);
            if (conv != null)
            {
                await _persistencia.RegistrarMensaje(conv.Id, $" [ID:{prodId}_{catId}_{page}] Esperando cantidad manual...", TipoRemitente.Sistema);
            }

            //await bot.AnswerCallbackQuery(callbackQuerry.Id, "Teclado activado. Escribe la cantidad a añadir.");
        }

        if (rf.StartsWith("add_prod_"))
        {
            //int prodId = int.Parse(action.Replace("add_prod_", ""));

            int prodId = int.Parse(parts[2]);
            int cantidad = int.Parse(parts[3]);
            int catId = int.Parse(parts[4]);
            int page = int.Parse(parts[5]);
            if (cantidad > 0)
            {
                var resultado = await _persistencia.AgregarProducto(callbackQuerry.From.Id, prodId, cantidad);
                if (resultado.Success)
                {
                    await bot.AnswerCallbackQuery(callbackQuerry.Id, resultado.msg);
                    await RenderizarCatalogo(bot, callbackQuerry, catId, page);
                }
                else
                    await bot.AnswerCallbackQuery(callbackQuerry.Id, $"Error: {resultado.msg}", showAlert: true);
            }
            else
                await bot.AnswerCallbackQuery(callbackQuerry.Id, $"Error: cantidad invalida", showAlert: true);
        }
        /*await (action switch
        {
            "pcat" => SendCategories(callbackQuerry.Message!.Chat.Id, int.Parse(parts[1]), callbackQuerry.Message.MessageId),
            "cat" => SendProducts(callbackQuerry.Message!.Chat.Id, int.Parse(parts[1]), 0, callbackQuerry.Message.MessageId),
            "pprod" => SendProducts(callbackQuerry.Message!.Chat.Id, int.Parse(parts[1]), int.Parse(parts[2]), callbackQuerry.Message.MessageId),
            _ => Task.CompletedTask
        });

        await bot.AnswerCallbackQuery(callbackQuerry.Id);*/
    }
    private async Task RenderizarCatalogo(ITelegramBotClient bot, CallbackQuery callbackQuerry, int catId, int page)
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
}
