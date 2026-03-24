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
                new[] { InlineKeyboardButton.WithCallbackData($"{p.Nombre} - ${p.Precio}", $"prod_{p.Id}") }).ToList();
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
}