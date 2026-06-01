using Telegram.Bot;
using Services.ChatBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace Services.ChatBot.Models;
/// <summary>
/// Provides utility methods for UI manipulation and message management within the Telegram bot.
/// Implements <see cref="IUtilsUI"/> to decouple bot logic from direct Telegram API interaction.
/// </summary>
public class UtilsModule(ITelegramBotClient bot, IConfiguration configuration) : IUtilsUI
{
    private readonly string url = configuration["ChatBotConfig:BannerUrl"];
    /// <summary>
    /// Invalidates an existing menu or session message by updating its content with a warning.
    /// This is typically used when a conversation session has expired or the user needs to restart.
    /// </summary>
    /// <param name="chatId">The unique identifier for the target chat.</param>
    /// <param name="messageId">The identifier of the message to invalidate.</param>
    /// <param name="textoAviso">The specific alert or reason text to display to the user.</param>
    /// <param name="action">Optional action parameter for future routing (not currently utilized).</param>
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

            await bot.EditMessageMedia(chatId, messageId, media);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al invalidar: {ex.Message}");
        }
    }
    /// <summary>
    /// Attempts to delete a specific message from a Telegram chat.
    /// </summary>
    /// <param name="chatId">The unique identifier for the target chat.</param>
    /// <param name="messageId">The identifier of the message to delete.</param>    
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
    /// <summary>
    /// Returns a null keyboard markup, which can be passed to Telegram methods to remove an existing inline keyboard.
    /// </summary>
    /// <returns>A null <see cref="InlineKeyboardMarkup"/>.</returns>
    public InlineKeyboardMarkup? LimpiarKeyboard() => null; // Retorna null para limpiar el teclado inline
}