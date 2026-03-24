using Telegram.Bot;
using Services.ChatBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Models;
public class UtilsModule(ITelegramBotClient bot) : IUtilsUI
{
    public async Task InvalidarMenu(long chatId, int messageId, string textoAviso)
    {
        try
        {
            await bot.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: $"Alert: {textoAviso}\nUsa el comando /start para iniciar una nueva compra",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                replyMarkup: null // Eliminar el teclado inline
            );
        }catch(Exception ex)
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