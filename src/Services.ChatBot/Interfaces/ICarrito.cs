using Shared.Core.Entities;
using Telegram.Bot.Types.ReplyMarkups;
namespace Services.ChatBot.Interfaces;

public interface ICarrito
{
    (string texto, InlineKeyboardMarkup markup) BuildUICarrito(Pedido? pedido);
}