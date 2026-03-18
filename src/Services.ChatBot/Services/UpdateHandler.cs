using Services.ChatBot.DTOs;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Services.ChatBot.Interfaces;

namespace Webhook.Controllers.Services;

public class UpdateHandler(ITelegramBotClient bot,
ILogger<UpdateHandler> logger,
IHttpClientFactory httpClientFactory,
IMenuUI menuUI,
ICatalogoUI catalogoUI
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
        if (text == "/start" || text.ToLower().Contains("Catalogo"))
        {
            CallbackQuery callbackQuerry = new CallbackQuery
            {
                Data = "pcat_0",
                Message = new Message
                {
                    Chat = msg.Chat
                }
            };

            _ = OnCallbackQuery(callbackQuerry);
        }
        else
        {
            await bot.SendMessage(msg.Chat, "Usa /start para ver el catalogo");
        }
        if (text == "/remove")
        {
            await RemoveKeyboard(msg);
        }
    }

    private async Task OnCallbackQuery(CallbackQuery callbackQuerry)
    {
        var rf = callbackQuerry.Data;
        if (string.IsNullOrEmpty(rf)) return;

        var parts = rf.Split('_');
        var action = parts[0];
        Console.WriteLine($"Chat {callbackQuerry.Message!.Chat}, MessageID {callbackQuerry.Message.MessageId}");
        //Console.WriteLine(action.ToString());
        if (action == "pcat")
        {
            int page = int.Parse(parts[1]);
            var data = await _gateway.GetFromJsonAsync<PagedResult<CategoriaDTO>>($"categorias/list-6?page={page}&pageSize=6");
            if (data == null || !data.Items.Any()) return;
            // Usamos la interfaz de categorías
            var markup = menuUI.BuildUICategorias(data, page);
            //Console.WriteLine($"Chat {callbackQuerry.Message!.Chat}, MessageID {callbackQuerry.Message.MessageId}, Markup {data.TotalCount}");
            if (callbackQuerry.Message.MessageId == null || callbackQuerry.Message.MessageId == 0)
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
