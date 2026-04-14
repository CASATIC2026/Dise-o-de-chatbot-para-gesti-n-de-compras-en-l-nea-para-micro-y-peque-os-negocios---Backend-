using Microsoft.AspNetCore.Mvc.RazorPages;
using Telegram.Bot.Types.ReplyMarkups;
using Services.ChatBot.DTOs;

namespace Services.ChatBot.Interfaces
{
    public interface ICatalogoUI
    {
        InlineKeyboardMarkup BuildUIProductos(PagedResult<ProductoDTO> data, int catId, int page);
        InlineKeyboardMarkup BuildUIDetalleProducto(int prodId, int catId, int page, int cantidadActual);
        InlineKeyboardMarkup BuildUICategorias(PagedResult<CategoriaDTO> data, int page);
        (InlineKeyboardMarkup markup, string texto) BuildUIPedidos(PagedResult<PedidoDTO> data, int page);
    }
}