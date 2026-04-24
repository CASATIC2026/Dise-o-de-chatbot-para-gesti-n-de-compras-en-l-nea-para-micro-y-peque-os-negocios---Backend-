using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Interfaces;

/// <summary>
/// Defines general utility operations for manipulating the Telegram Bot's user interface and message history.
/// </summary>
public interface IUtilsUI
{
    /// <summary>
    /// Updates an existing message to indicate that the current menu or session is no longer valid.
    /// Typically used to handle timeouts or process flow interruptions.
    /// </summary>
    /// <param name="chatId">The unique identifier for the target chat.</param>
    /// <param name="messageId">The identifier of the message to invalidate.</param>
    /// <param name="textoAviso">The specific reason or alert text to show the user.</param>
    /// <param name="action">Optional context or action identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvalidarMenu(long chatId, int messageId, string textoAviso, string action);

    /// <summary>
    /// Deletes a specific message from the chat history.
    /// </summary>
    /// <param name="chatId">The unique identifier for the target chat.</param>
    /// <param name="messageId">The identifier of the message to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EliminarMensaje(long chatId, int messageId);
    
    /// <summary>
    /// Returns a null keyboard markup used to remove an inline keyboard from a message.
    /// </summary>
    /// <returns>A null <see cref="InlineKeyboardMarkup"/>.</returns>
    InlineKeyboardMarkup? LimpiarKeyboard();
}
