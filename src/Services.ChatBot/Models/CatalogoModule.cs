using System.Text;
using Services.ChatBot.DTOs;
using Services.ChatBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Models;

/// <summary>
/// Implementation of <see cref="ICatalogoUI"/> responsible for generating the interactive 
/// Telegram UI components for browsing categories, products, and order history.
/// </summary>
public class CatalogoModule : ICatalogoUI
{
    /// <summary>
    /// Builds an inline keyboard containing a paginated list of product categories.
    /// </summary>
    /// <param name="data">The paginated result containing category DTOs.</param>
    /// <param name="page">The current page index for navigation metadata.</param>
    /// <returns>An <see cref="InlineKeyboardMarkup"/> with category buttons and pagination controls.</returns>
    public InlineKeyboardMarkup BuildUICategorias(PagedResult<CategoriaDTO> data, int page)
    {
        var buttons = new List<InlineKeyboardButton[]>();
        if (data != null && data.Items.Any())
        {
            buttons = data.Items.Select(c =>
            new[] { InlineKeyboardButton.WithCallbackData(c.Nombre, $"cat_{c.Id}") }).ToList();

            var navRow = new List<InlineKeyboardButton>();
            if (page > 0) navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"pcat_{page - 1}"));
            if ((page + 1) * 6 < data.TotalCount) navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"pcat_{page + 1}"));

            if (navRow.Any()) buttons.Add(navRow.ToArray());
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Menu", "menu")]);
        }
        else
        {
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Menu", "menu")]);
        }
        return new InlineKeyboardMarkup(buttons);
    }

    /// <summary>
    /// Builds an inline keyboard containing a paginated list of products for a specific category.
    /// </summary>
    /// <param name="data">The paginated result containing product DTOs.</param>
    /// <param name="catId">The identifier of the category being browsed.</param>
    /// <param name="page">The current page index for navigation metadata.</param>
    /// <returns>An <see cref="InlineKeyboardMarkup"/> with product selection and pagination controls.</returns>
    public InlineKeyboardMarkup BuildUIProductos(PagedResult<ProductoDTO> data, int catId, int page)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        if (data != null && data.Items.Any())
        {
            buttons = data.Items.Select(p =>
                new[] { InlineKeyboardButton.WithCallbackData($"{p.Nombre} - ${p.Precio}", $"prod_{p.Id}_{catId}_{page}") }).ToList();
            var navRow = new List<InlineKeyboardButton>();
            if (page > 0) navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"pprod_{catId}_{page - 1}"));
            if ((page + 1) * 4 < data.TotalCount) navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"pprod_{catId}_{page + 1}"));

            if (navRow.Any()) buttons.Add(navRow.ToArray());
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Categorías", "pcat_0")]);
        }
        else
        {
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Categorías", "pcat_0")]);
        }
        return new InlineKeyboardMarkup(buttons);
    }

    /// <summary>
    /// Builds the interactive interface for a product's detailed view.
    /// This includes quantity adjustment controls (+/-), manual entry (✏️), and the action button 
    /// (Add to Cart or Confirm Changes depending on context).
    /// </summary>
    /// <param name="prodId">The unique identifier of the product.</param>
    /// <param name="catId">The category identifier. If -1, the UI adapts for "Edit from Cart" mode.</param>
    /// <param name="page">The page index used for returning to the previous view.</param>
    /// <param name="cantidadActual">The currently selected quantity for the UI display.</param>
    /// <returns>An <see cref="InlineKeyboardMarkup"/> with product interaction controls.</returns>
    public InlineKeyboardMarkup BuildUIDetalleProducto(int prodId, int catId, int page, int cantidadActual)
    {
        var buttons = new List<InlineKeyboardButton[]>
        {
            new[]{
                    InlineKeyboardButton.WithCallbackData("-", $"dec_{prodId}_{catId}_{page}"),
                    InlineKeyboardButton.WithCallbackData($"{cantidadActual}", $"none"),
                    InlineKeyboardButton.WithCallbackData("✏️", $"edit_qty_{prodId}_{catId}_{page}_{cantidadActual}"),
                    InlineKeyboardButton.WithCallbackData("+", $"inc_{prodId}_{catId}_{page}")
            }
        };
        if (catId == -1)

        {
            buttons.Add([InlineKeyboardButton.WithCallbackData("✅ Confirmar Cambios", $"upd_prod_{prodId}_{cantidadActual}")]);
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Volver al Carrito", $"cart")]);
        }
        else
        {
            buttons.Add([InlineKeyboardButton.WithCallbackData("🛒 Añadir al Carrito", $"add_prod_{prodId}_{cantidadActual}_{catId}_{page}")]);
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Volver a Productos", $"cat_{catId}_{page}")]);
        }


        return new InlineKeyboardMarkup(buttons);

    }

    /// <summary>
    /// Builds a text-based summary and navigation keyboard for the user's order history.
    /// Uses a fixed-width Markdown block to render a tabular view of orders.
    /// </summary>
    /// <param name="data">The paginated result containing the user's past orders.</param>
    /// <param name="page">The current page index for history navigation.</param>
    /// <returns>A tuple containing the navigation <see cref="InlineKeyboardMarkup"/> and the formatted summary string.</returns>
    public (InlineKeyboardMarkup markup, string texto) BuildUIPedidos(PagedResult<PedidoDTO> data, int page)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        var sb = new StringBuilder();
        sb.AppendLine("```");
        sb.AppendLine("PEDIDOS REALIZADOS");
        sb.AppendLine(new string('-', 40) + "\n");
        if (data == null)
        {
            sb.AppendLine("No se encontraron pedidos");
            sb.AppendLine("```");
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Menu", "menu")]);
            var markup = new InlineKeyboardMarkup(buttons);
            return (markup, sb.ToString());
        }
        else
        {
            sb.AppendLine($"{"ID",-4} {"FECHA",-12} {"ESTADO",-12} {"TOTAL",8}");
            sb.AppendLine(new string('-', 40));

            foreach (var pedido in data.Items)
            {
                string valor = Convert.ToString(pedido.Total!.Value);
                decimal valorD = Convert.ToDecimal(valor);
                string total = $"${pedido.Total!.Value.ToString("F2")}";

                sb.AppendLine($"{pedido.Id,-4} " +
                              $"{pedido.FechaRealizado.ToString("dd/MM/yyyy"),-12} " +
                              $"{pedido.Estado.ToString().Trim()[..Math.Min(12, pedido.Estado.ToString().Length)],-12} " +
                              $"{total,8}");
            }

            sb.AppendLine("```"); // Cerramos el bloque        

            var navRow = new List<InlineKeyboardButton>();
            if (page > 0) navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"pords_{page - 1}"));
            if ((page + 1) * 6 < data.TotalCount) navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"pords_{page + 1}"));
            if (navRow.Count != 0) buttons.Add([.. navRow]);
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Menu", "menu")]);
            var markup = new InlineKeyboardMarkup(buttons);
            return new(markup, sb.ToString());
        }
    }
}
