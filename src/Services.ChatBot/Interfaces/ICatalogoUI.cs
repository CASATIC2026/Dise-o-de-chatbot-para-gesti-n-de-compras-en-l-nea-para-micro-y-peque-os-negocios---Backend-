using Microsoft.AspNetCore.Mvc.RazorPages;
using Telegram.Bot.Types.ReplyMarkups;
using Services.ChatBot.DTOs;

namespace Services.ChatBot.Interfaces
{
    /// <summary>
    /// Defines the contract for building user interface components related to the product catalog.
    /// This includes rendering product lists, product details, categories, and order history.
    /// </summary>
    public interface ICatalogoUI
    {
        /// <summary>
        /// Builds the inline keyboard markup for displaying a paginated list of products.
        /// </summary>
        /// <param name="data">A <see cref="PagedResult{ProductoDTO}"/> containing the products for the current page.</param>
        /// <param name="catId">The ID of the current category being viewed.</param>
        /// <param name="page">The current page number.</param>
        /// <returns>An <see cref="InlineKeyboardMarkup"/> for product navigation and selection.</returns>
        InlineKeyboardMarkup BuildUIProductos(PagedResult<ProductoDTO> data, int catId, int page);

        /// <summary>
        /// Builds the inline keyboard markup for displaying the detailed view of a single product.
        /// Includes options for quantity adjustment and adding to cart.
        /// </summary>
        /// <param name="prodId">The ID of the product being displayed.</param>
        /// <param name="catId">The ID of the category the product belongs to.</param>
        /// <param name="page">The page number from the catalog where the product was selected.</param>
        /// <param name="cantidadActual">The current quantity selected for the product.</param>
        /// <returns>An <see cref="InlineKeyboardMarkup"/> for product detail interaction.</returns>
        InlineKeyboardMarkup BuildUIDetalleProducto(int prodId, int catId, int page, int cantidadActual);

        /// <summary>
        /// Builds the inline keyboard markup for displaying a paginated list of product categories.
        /// </summary>
        /// <param name="data">A <see cref="PagedResult{CategoriaDTO}"/> containing the categories for the current page.</param>
        /// <param name="page">The current page number.</param>
        /// <returns>An <see cref="InlineKeyboardMarkup"/> for category navigation.</returns>
        InlineKeyboardMarkup BuildUICategorias(PagedResult<CategoriaDTO> data, int page);

        /// <summary>
        /// Builds the inline keyboard markup and message text for displaying a paginated list of user orders.
        /// </summary>
        /// <param name="data">A <see cref="PagedResult{PedidoDTO}"/> containing the orders for the current page.</param>
        /// <param name="page">The current page number.</param>
        /// <returns>A tuple containing the <see cref="InlineKeyboardMarkup"/> for order navigation and the formatted message text.</returns>
        (InlineKeyboardMarkup markup, string texto) BuildUIPedidos(PagedResult<PedidoDTO> data, int page);
    }
}