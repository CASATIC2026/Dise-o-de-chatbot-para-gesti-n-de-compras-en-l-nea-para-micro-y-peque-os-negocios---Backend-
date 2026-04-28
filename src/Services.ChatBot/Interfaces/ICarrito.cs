using Shared.Core.Entities;
using Telegram.Bot.Types.ReplyMarkups;
using Services.ChatBot.DTOs;

namespace Services.ChatBot.Interfaces;

/// <summary>
/// Defines the contract for building user interface components related to the shopping cart and checkout process.
/// This interface decouples the UI layout logic from the bot's interaction handlers.
/// </summary>
public interface ICarrito
{
    /// <summary>
    /// Builds the user interface for the current shopping cart view.
    /// </summary>
    /// <param name="pedido">The active order representing the cart contents. Can be null if the cart is empty.</param>
    /// <returns>A tuple containing the formatted message text and the <see cref="InlineKeyboardMarkup"/> for cart management.</returns>
    (string texto, InlineKeyboardMarkup markup) BuildUICarrito(Pedido? pedido);

    /// <summary>
    /// Builds the final checkout summary user interface, including order details and delivery information.
    /// </summary>
    /// <param name="pedido">The pending order to be confirmed.</param>
    /// <param name="cliente">The client information containing delivery details like address and phone.</param>
    /// <returns>A tuple containing the summary message text and the <see cref="InlineKeyboardMarkup"/> for final confirmation.</returns>
    (string texto, InlineKeyboardMarkup markup) BuildUIResumenFinal(Pedido? pedido, Cliente? cliente);

    (string texto, InlineKeyboardMarkup markup) Ticket(PagosLinksDTO? pagosLinks, int pedidoId, string url);
}