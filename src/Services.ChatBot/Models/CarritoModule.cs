using System.Text;
using Services.ChatBot.Interfaces;
using Shared.Core.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Models;

public class CarritoModule : ICarrito
{
    public (string texto, InlineKeyboardMarkup markup) BuildUICarrito(Pedido? pedido)
    {
        if (pedido == null || !pedido.PedidoProductos.Any())
        {
            string msg = "🛒 *Tu carrito está vacío. Agrega productos para verlos aquí.";
            var emptyKbd = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("🛍 Ir al Catálogo", "pcat_0")
            );
            return (msg, emptyKbd);
        }

        var sb = new StringBuilder();
        sb.AppendLine("🛒 *RESUMEN DE COMPRA*");
        sb.AppendLine("-------------------------\n");

        decimal total = 0;
        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var pp in pedido.PedidoProductos)
        {
            decimal subtotal = pp.Cantidad * pp.PrecioUnitario;
            total += subtotal;

            sb.AppendLine($"🔹 *{pp.Producto.Nombre}*");
            sb.AppendLine($"Cantidad: {pp.Cantidad} \t Precio Unitario: ${pp.PrecioUnitario}");
            sb.AppendLine($"\tSubtotal: ${subtotal}\n");
            sb.AppendLine();

            buttons.Add(
                [
                    InlineKeyboardButton.WithCallbackData($"Eliminar {pp.Producto.Nombre}", $"ask_rmv_{pp.ProductoId}_0_0"),
                    InlineKeyboardButton.WithCallbackData($"✏️", $"prod_{pp.ProductoId}_{-1}_0_{pp.Cantidad}")
                ]
            );
        }

        sb.AppendLine("-------------------------");
        sb.AppendLine($"💰*TOTAL: ${total}*");

        buttons.Add(
            [
                InlineKeyboardButton.WithCallbackData("🗑 Vaciar", "ask_clear"),
                InlineKeyboardButton.WithCallbackData("✅ Finalizar Compra", "checkout")
            ]
        );
        buttons.Add([InlineKeyboardButton.WithCallbackData("Seguir Comprando", "pcat_0")]);
        buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Menu", "menu")]);

        return (sb.ToString(), new InlineKeyboardMarkup(buttons));
    }
}