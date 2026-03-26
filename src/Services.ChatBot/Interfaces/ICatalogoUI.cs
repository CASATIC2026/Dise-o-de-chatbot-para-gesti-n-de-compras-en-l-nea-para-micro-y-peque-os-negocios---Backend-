using Microsoft.AspNetCore.Mvc.RazorPages;
using Telegram.Bot.Types.ReplyMarkups;
using Services.ChatBot.DTOs;

namespace Services.ChatBot.Interfaces
{
    public interface ICatalogoUI
    {
        InlineKeyboardMarkup BuildUIProductos(PagedResult<ProductoDTO> data, int catId, int page);
        InlineKeyboardMarkup BuildUIDetalleProducto(int prodId, int catId, int page, int cantidadActual);
    }
}