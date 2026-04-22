using Telegram.Bot;
using Services.ChatBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace Services.ChatBot.Models;

public class UtilsModule(ITelegramBotClient bot) : IUtilsUI
{
    private readonly string url = "https://placehold.co/360x100/png?text=Tienda";
    public async Task InvalidarMenu(long chatId, int messageId, string textoAviso, string action)
    {
        try
        {
            var caption = $"Alert: {textoAviso}\nUsa el comando /start para iniciar una nueva compra";
            var media = new InputMediaPhoto(url)
            {
                Caption = caption,
                ParseMode = ParseMode.Markdown
            };

            //await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, caption, replyMarkup: markup);
            await bot.EditMessageMedia(chatId, messageId, media);
            /*await bot.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: $"Alert: {textoAviso}\nUsa el comando /start para iniciar una nueva compra",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                replyMarkup: null // Eliminar el teclado inline  
            );*/
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al invalidar: {ex.Message}");
        }
    }

    public async Task EliminarMensaje(long chatId, int messageId)
    {
        try
        {
            await bot.DeleteMessage(chatId, messageId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar mensaje: {ex.Message}");
        }
    }

    public InlineKeyboardMarkup? LimpiarKeyboard() => null; // Retorna null para limpiar el teclado inline
}