using Services.ChatBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;
using Services.ChatBot.DTOs;

namespace Services.ChatBot.Models;

public class CategoriasModule : IMenuUI
{
    public InlineKeyboardMarkup BuildUICategorias(PagedResult<CategoriaDTO> data, int page)
    {
        var buttons = data.Items.Select(c =>
            new[] { InlineKeyboardButton.WithCallbackData(c.Nombre, $"cat_{c.Id}") }).ToList();

        var navRow = new List<InlineKeyboardButton>();
        if (page > 0) navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"pcat_{page - 1}"));
        if ((page + 1) * 6 < data.TotalCount) navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"pcat_{page + 1}"));

        if (navRow.Any()) buttons.Add(navRow.ToArray());
        return new InlineKeyboardMarkup(buttons);
    }
}