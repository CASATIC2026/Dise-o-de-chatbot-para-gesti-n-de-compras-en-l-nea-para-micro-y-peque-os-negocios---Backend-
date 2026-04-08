using Services.ChatBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;
using Services.ChatBot.DTOs;

namespace Services.ChatBot.Models;

public class MenuModule : IMenuUI
{
    public InlineKeyboardMarkup BuildUIHome(string userName)
    {
        var buttons = new List<InlineKeyboardButton[]>{        
           ([InlineKeyboardButton.WithCallbackData("🛍 Ver Catalogo", "pcat_0")]),
            ([InlineKeyboardButton.WithCallbackData("🛒 Mi Carrito", "cart")]),
            ([InlineKeyboardButton.WithCallbackData("📦 Mis Pedidos", "ords")]),
            ([InlineKeyboardButton.WithCallbackData("❓ Ayuda", "hlp")])
        };    
        return new InlineKeyboardMarkup(buttons);
    }
}