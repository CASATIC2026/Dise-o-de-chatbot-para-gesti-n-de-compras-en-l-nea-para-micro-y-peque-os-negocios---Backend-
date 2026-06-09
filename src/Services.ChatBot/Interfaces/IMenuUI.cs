using Services.ChatBot.DTOs;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Interfaces
{
    /// <summary>
    /// Defines the contract for building user interface components related to the main menu of the bot.
    /// </summary>
    public interface IMenuUI
    {
        /// <summary>
        /// Builds the inline keyboard markup for the bot's home/main menu.
        /// </summary>
        /// <param name="userName">The name of the user to personalize the greeting or menu.</param>
        /// <returns>An <see cref="InlineKeyboardMarkup"/> representing the main menu options.</returns>
        InlineKeyboardMarkup BuildUIHome(string userName);
    }
}
