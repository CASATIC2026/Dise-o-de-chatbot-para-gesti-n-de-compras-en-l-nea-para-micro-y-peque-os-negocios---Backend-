using Services.ChatBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;
using Services.ChatBot.DTOs;

namespace Services.ChatBot.Models;

/// <summary>
/// Implementation of <see cref="IMenuUI"/> that handles the construction of 
/// the main menu user interface for the Telegram bot.
/// </summary>
public class MenuModule : IMenuUI
{
    /// <summary>
    /// Builds the main menu (Home) inline keyboard with options for the catalog, 
    /// shopping cart, order history, and help.
    /// </summary>
    /// <param name="userName">The name of the user used to personalize the interaction.</param>
    /// <returns>An <see cref="InlineKeyboardMarkup"/> containing the primary navigation buttons.</returns>
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