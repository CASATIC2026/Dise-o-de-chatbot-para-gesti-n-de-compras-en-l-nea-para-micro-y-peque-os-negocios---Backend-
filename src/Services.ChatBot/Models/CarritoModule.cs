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

    public (string texto, InlineKeyboardMarkup markup) BuildUIResumenFinal(Pedido? pedido, Cliente? cliente)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🏁 *VERIFICA TU PEDIDO*");
        sb.AppendLine("__________________________\n");
        if (cliente == null || pedido == null) return (sb.ToString(), null);

        // 1. Listado resumido de productos
        foreach (var item in pedido.PedidoProductos)
        {
            sb.AppendLine($"▪️ {item.Producto.Nombre} x{item.Cantidad} — *${item.Cantidad * item.PrecioUnitario}*");
        }

        sb.AppendLine("\n__________________________");
        sb.AppendLine($"💰 *TOTAL A PAGAR: ${pedido.Total}*");
        sb.AppendLine("__________________________\n");

        // 2. Datos de entrega
        sb.AppendLine("📍 *Dirección de Envío:*");
        sb.AppendLine($"`{cliente.Direccion}`");
        sb.AppendLine($"\n📞 *Teléfono:* `{cliente.Telefono}`");
        sb.AppendLine("__________________________\n");
        sb.AppendLine("¿Toda la información es correcta?");

        // 3. Botones de acción
        var buttons = new List<InlineKeyboardButton[]>
    {
        new[] {
            InlineKeyboardButton.WithCallbackData("🚀 CONFIRMAR Y PAGAR", "menu"),
        },
        new[] { 
            // Si algo está mal, lo regresamos al inicio del checkout para que sobrescriba los datos
            InlineKeyboardButton.WithCallbackData("🔄 Corregir Datos", "checkout")
        },
        new[] {
            InlineKeyboardButton.WithCallbackData("🛒 Volver al Carrito", "cart")
        }
    };
        return (sb.ToString(), new InlineKeyboardMarkup(buttons));
    }
}