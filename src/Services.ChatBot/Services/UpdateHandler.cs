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
using Shared.Core.Entities;
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
        if(conv == null) return;

        var lastMsg = await context.Mensajes.Where(m => 
        m.ConversacionId == conv.Id)
        .OrderByDescending(m => m.FechaEnvio)
        .FirstOrDefaultAsync();
        
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

            //var enviado = await bot.SendMessage(msg.Chat, "📂 Menú:",OnCallbackQuery(callbackQuerry));
            //var enviado = OnCallbackQuery(callbackQuerry);

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
        if (text == "/remove")
        {
            await RemoveKeyboard(msg);
        }
        if(lastMsg != null && lastMsg.Remitente == TipoRemitente.Sistema && lastMsg.Contenido.Contains("[ID:")){
            if (int.TryParse(text, out int cantidad) && cantidad > 0)
            {
                int prodId = int.Parse(lastMsg.Contenido.Split(":")[1].Split("]")[0]);
                var res = await _persistencia.AgregarProducto(msg.From.Id, prodId, cantidad);

                await bot.SendMessage(msg.Chat, res.msg);
                return;
            }
            else
            {
                await bot.SendMessage(msg.Chat.Id, "Valor invalido. Por favor, solo numeros mayores a 0.");
                return;
            }
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
        if(action == "prod")
        {
            int prodId = int.Parse(parts[1]);
            int catId = int.Parse(parts[2]);
            int page = int.Parse(parts[3]);

            var data = await _gateway.GetFromJsonAsync<ProductoDTO>($"productos/{prodId}");

            string msg = $"📦 {data.Nombre}\n"+ 
            $"\n\tPrecio: ${data.Precio}"+
            $"\n\tStock: {data.StockDisponible}";
            
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("-", $"dec_{prodId}"),
                    InlineKeyboardButton.WithCallbackData("1", $"none"),
                    InlineKeyboardButton.WithCallbackData("✏️", $"edit_qty_{prodId}"),
                    InlineKeyboardButton.WithCallbackData("+", $"inc_{prodId}")
                },

                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Anadir", $"add_prod_{prodId}_1")
                },

                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Volver", $"cat_{catId}_{page}")
                }
            });

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
            else if( currentQty > 1) currentQty--;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("-", $"dec_{prodId}"),
                    InlineKeyboardButton.WithCallbackData($"{currentQty}", $"none"),
                    InlineKeyboardButton.WithCallbackData("✏️", $"edit_qty_{prodId}"),
                    InlineKeyboardButton.WithCallbackData("+", $"inc_{prodId}")
                },

                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"Anadir {currentQty}", $"add_prod_{prodId}_{currentQty}")
                },

                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Volver", $"cat_{catId}_{page}")
                }
            });

            await bot.EditMessageReplyMarkup(callbackQuerry.Message.Chat.Id, callbackQuerry.Message.MessageId, keyboard);
        }
        if( action == "edit_qty")
        {
            int prodId = int.Parse(parts[1]);

            string instruction = $"[ID: {prodId}] Por favor, ingresa la cantidad deseada para el producto";

            var send = await bot.SendMessage(callbackQuerry.Message!.Chat, instruction);

            var conv = await _persistencia.ObtenerConversacionActiva(callbackQuerry.From.Id);
            if(conv != null)
            {
                await _persistencia.RegistrarMensaje(conv.Id, instruction, TipoRemitente.Sistema);
            }            
        }
        if (rf.StartsWith("add_prod_"))
        {
            //int prodId = int.Parse(action.Replace("add_prod_", ""));
            
            int prodId = int.Parse(parts[2]);
            int cantidad = int.Parse(parts[3]);
            var resultado = await _persistencia.AgregarProducto(callbackQuerry.From.Id, prodId, cantidad);
            if (resultado.Success)
                await bot.AnswerCallbackQuery(callbackQuerry.Id, resultado.msg);
            else
                await bot.AnswerCallbackQuery(callbackQuerry.Id, $"Error: {resultado.msg}", showAlert: true);
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

    /*
    
    

    private async Task SendCategories(long chatId, int page, int? messageId = null)
    {
        var response = await _gateway.GetFromJsonAsync<PagedResult<CategoriaDTO>>($"categorias/list-6?page={page}&pageSize=6");

        if (response == null || !response.Items.Any()) return;

        var buttons = response.Items.Select(c =>
        new[] { InlineKeyboardButton.WithCallbackData(c.Nombre, $"cat_{c.Id}") }).ToList();

        //navegacion 
        var navRow = new List<InlineKeyboardButton>();
        if (page > 0) navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"pcat_{page - 1}"));
        if ((page + 1) * 4 < response.TotalCount) navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"pcat_{page + 1}"));

        if (navRow.Any()) buttons.Add(navRow.ToArray());

        string text = $"📂 *Categorías* (Página {page + 1})\nSelecciona una para ver productos:";

        if (messageId.HasValue)
        {
            await bot.EditMessageText(chatId, messageId.Value, text, parseMode: ParseMode.Markdown, replyMarkup: new InlineKeyboardMarkup(buttons));
        }
        else
        {
            await bot.SendMessage(chatId, text, parseMode: ParseMode.Markdown, replyMarkup: new InlineKeyboardMarkup(buttons));
        }
    }

    /*public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message } => OnMessage(message),
            { EditedMessage: { } message } => OnMessage(message),
            { CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery),
            { InlineQuery: { } inlineQuery } => OnInlineQuery(inlineQuery),
            { ChosenInlineResult: { } chosenInlineResult } => OnChosenInlineResult(chosenInlineResult),
            { Poll: { } poll } => OnPoll(poll),
            { PollAnswer: { } pollAnswer } => OnPollAnswer(pollAnswer),
            // ChannelPost:
            // EditedChannelPost:
            // ShippingQuery:
            // PreCheckoutQuery:
            _ => UnknownUpdateHandlerAsync(update)
        });
    }

    private async Task OnMessage(Message msg)
    {
        logger.LogInformation("Receive message type: {MessageType}", msg.Type);
        if (msg.Text is not { } messageText)
            return;

        Message sentMessage = await (messageText.Split(' ')[0] switch
        {
            "/photo" => SendPhoto(msg),
            "/inline_buttons" => SendInlineKeyboard(msg),
            "/keyboard" => SendReplyKeyboard(msg),
            "/remove" => RemoveKeyboard(msg),
            "/request" => RequestContactAndLocation(msg),
            "/inline_mode" => StartInlineQuery(msg),
            "/poll" => SendPoll(msg),
            "/poll_anonymous" => SendAnonymousPoll(msg),
            "/throw" => FailingHandler(msg),
            _ => Usage(msg)
        });
        logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.Id);
    }

    async Task<Message> Usage(Message msg)
    {
        const string usage = """
                <b><u>Bot menu</u></b>:
                /photo          - send a photo
                /inline_buttons - send inline buttons
                /keyboard       - send keyboard buttons
                /remove         - remove keyboard buttons
                /request        - request location or contact
                /inline_mode    - send inline-mode results list
                /poll           - send a poll
                /poll_anonymous - send an anonymous poll
                /throw          - what happens if handler fails
            """;
        return await bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> SendPhoto(Message msg)
    {
        await bot.SendChatAction(msg.Chat, ChatAction.UploadPhoto);
        await Task.Delay(2000); // simulate a long task
        await using var fileStream = new FileStream("Files/bot.gif", FileMode.Open, FileAccess.Read);
        return await bot.SendPhoto(msg.Chat, fileStream, caption: "Read https://telegrambots.github.io/book/");
    }

    // Send inline keyboard. You can process responses in OnCallbackQuery handler
    async Task<Message> SendInlineKeyboard(Message msg)
    {
        return await bot.SendMessage(msg.Chat, "Inline buttons:", replyMarkup: new InlineKeyboardButton[][] {
                ["1.1", "1.2", "1.3"],
                [("WithCallbackData", "CallbackData"), ("WithUrl", "https://github.com/TelegramBots/Telegram.Bot")]
            });
    }

    async Task<Message> SendReplyKeyboard(Message msg)
    {
        return await bot.SendMessage(msg.Chat, "Keyboard buttons:", replyMarkup: new string[][] { ["1.1", "1.2", "1.3"], ["2.1", "2.2"] });
    }

    async Task<Message> RemoveKeyboard(Message msg)
    {
        return await bot.SendMessage(msg.Chat, "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> RequestContactAndLocation(Message msg)
    {
        var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddButton(KeyboardButton.WithRequestLocation("Location"))
            .AddButton(KeyboardButton.WithRequestContact("Contact"));
        return await bot.SendMessage(msg.Chat, "Who or Where are you?", replyMarkup: replyMarkup);
    }

    async Task<Message> StartInlineQuery(Message msg)
    {
        var button = InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode");
        return await bot.SendMessage(msg.Chat, "Press the button to start Inline Query\n\n" +
            "(Make sure you enabled Inline Mode in @BotFather)", replyMarkup: new InlineKeyboardMarkup(button));
    }

    async Task<Message> SendPoll(Message msg)
    {
        return await bot.SendPoll(msg.Chat, "Question", PollOptions, isAnonymous: false);
    }

    async Task<Message> SendAnonymousPoll(Message msg)
    {
        return await bot.SendPoll(chatId: msg.Chat, "Question", PollOptions);
    }

    static Task<Message> FailingHandler(Message msg)
    {
        throw new NotImplementedException("FailingHandler");
    }

    // Process Inline Keyboard callback data
    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        await bot.AnswerCallbackQuery(callbackQuery.Id, $"Received {callbackQuery.Data}");
        await bot.SendMessage(callbackQuery.Message!.Chat, $"Received {callbackQuery.Data}");
    }

    #region Inline Mode

    private async Task OnInlineQuery(InlineQuery inlineQuery)
    {
        logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = [ // displayed result
            new InlineQueryResultArticle("1", "Telegram.Bot", new InputTextMessageContent("hello")),
            new InlineQueryResultArticle("2", "is the best", new InputTextMessageContent("world"))
        ];
        await bot.AnswerInlineQuery(inlineQuery.Id, results, cacheTime: 0, isPersonal: true);
    }

    private async Task OnChosenInlineResult(ChosenInlineResult chosenInlineResult)
    {
        logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);
        await bot.SendMessage(chosenInlineResult.From.Id, $"You chose result with Id: {chosenInlineResult.ResultId}");
    }

    #endregion

    private Task OnPoll(Poll poll)
    {
        logger.LogInformation("Received Poll info: {Question}", poll.Question);
        return Task.CompletedTask;
    }

    private async Task OnPollAnswer(PollAnswer pollAnswer)
    {
        var answer = pollAnswer.OptionIds.FirstOrDefault();
        var selectedOption = PollOptions[answer];
        if (pollAnswer.User != null)
            await bot.SendMessage(pollAnswer.User.Id, $"You've chosen: {selectedOption.Text} in poll");
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }*/
}
