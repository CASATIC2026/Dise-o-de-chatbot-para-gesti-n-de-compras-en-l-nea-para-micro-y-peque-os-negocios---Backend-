using Services.ChatBot.DTOs;
using Services.ChatBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Models;

public class ProductosModule : ICatalogoUI
{
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
        return new InlineKeyboardMarkup(new[]
        {
            new[]
                {
                    InlineKeyboardButton.WithCallbackData("-", $"dec_{prodId}_{catId}_{page}"),
                    InlineKeyboardButton.WithCallbackData($"{cantidadActual}", $"none"),
                    InlineKeyboardButton.WithCallbackData("✏️", $"edit_qty_{prodId}_{catId}_{page}"),
                    InlineKeyboardButton.WithCallbackData("+", $"inc_{prodId}_{catId}_{page}")
                },

                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Anadir", $"add_prod_{prodId}_{cantidadActual}_{catId}_{page}")
                },

                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Volver", $"cat_{catId}_{page}")
                }
        });
    }
}