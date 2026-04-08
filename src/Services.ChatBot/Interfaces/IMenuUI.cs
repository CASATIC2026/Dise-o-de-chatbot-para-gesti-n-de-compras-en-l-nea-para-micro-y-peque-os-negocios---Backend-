using Services.ChatBot.DTOs;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Interfaces
{
    public interface IMenuUI
    {
        
        InlineKeyboardMarkup BuildUIHome(string userName);
    }
}

