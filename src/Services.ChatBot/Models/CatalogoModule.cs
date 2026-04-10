using System.Text;
using Services.ChatBot.DTOs;
using Services.ChatBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Models;

public class CatalogoModule : ICatalogoUI
{
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
    public InlineKeyboardMarkup BuildUIProductos(PagedResult<ProductoDTO> data, int catId, int page)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        if (data != null && data.Items.Any())
        {
            buttons = data.Items.Select(p =>
                new[] { InlineKeyboardButton.WithCallbackData($"{p.Nombre} - ${p.Precio}", $"prod_{p.Id}_{catId}_{page}") }).ToList();
            var navRow = new List<InlineKeyboardButton>();
            if (page > 0) navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"pprod_{catId}_{page - 1}"));
            if ((page + 1) * 6 < data.TotalCount) navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"pprod_{catId}_{page + 1}"));

            if (navRow.Any()) buttons.Add(navRow.ToArray());
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Categorías", "pcat_0")]);
        }
        else
        {
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Categorías", "pcat_0")]);
        }
        return new InlineKeyboardMarkup(buttons);
    }

    public InlineKeyboardMarkup BuildUIDetalleProducto(int prodId, int catId, int page, int cantidadActual)
    {

        var buttons = new List<InlineKeyboardButton[]>

        var buttons = new List<InlineKeyboardButton[]>
        {
            new[]{
                    InlineKeyboardButton.WithCallbackData("-", $"dec_{prodId}_{catId}_{page}"),
                    InlineKeyboardButton.WithCallbackData($"{cantidadActual}", $"none"),
                    InlineKeyboardButton.WithCallbackData("✏️", $"edit_qty_{prodId}_{catId}_{page}"),
                    InlineKeyboardButton.WithCallbackData("+", $"inc_{prodId}_{catId}_{page}")
            }
            }
        };


        if (catId == -1)
        {
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

    public (InlineKeyboardMarkup markup, string texto) BuildUIPedidos(PagedResult<PedidoDTO> data, int page)
    {
        var sb = new StringBuilder();
        string formato = "{0,-4} {1,-12}, {2,-15}, {3,}";
        if (data == null) return (null, sb.ToString());
        //Console.WriteLine($"Pagina {page}");

        sb.AppendLine("```");
        sb.AppendLine("PEDIDOS REALIZADOS");
        sb.AppendLine(new string('-', 40) + "\n");
        sb.AppendLine($"{"ID",-4} {"FECHA",-12} {"ESTADO",-12} {"TOTAL",8}");
        sb.AppendLine(new string('-', 40));

        foreach (var pedido in data.Items)
        {
            string total = '$' + pedido.Total.ToString("F2");

            sb.AppendLine($"{pedido.Id,-4} " +
                          $"{pedido.FechaRealizado.ToString("dd/MM/yyyy"),-12} " +
                          $"{pedido.Estado.Trim()[..Math.Min(12, pedido.Estado.Length)],-12} " +
                          $"{total,8}");
        }

        sb.AppendLine("```"); // Cerramos el bloque        
        var buttons = new List<InlineKeyboardButton[]>();
        var navRow = new List<InlineKeyboardButton>();
        if (page > 0) navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"pords_{page - 1}"));
        if ((page + 1) * 6 < data.TotalCount) navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"pords_{page + 1}"));
        if (navRow.Count != 0) buttons.Add([.. navRow]);

        buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Menu", "menu")]);
        var markup = new InlineKeyboardMarkup(buttons);
        return new(markup, sb.ToString());
    }

}
