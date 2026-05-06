using System.Text;
using System.Text.Json;
using Humanizer;
using Services.ChatBot.DTOs;
using Services.ChatBot.Interfaces;
using Shared.Core.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Models;

/// <summary>
/// Implementation of <see cref="ICarrito"/> that handles the construction of 
/// user interface components for the shopping cart and final checkout summary.
/// </summary>
public class CarritoModule : ICarrito
{
    /// <summary>
    /// Constructs the visual representation of the shopping cart, including the list of items,
    /// subtotal, total, and management buttons (edit, remove, clear, checkout).
    /// </summary>
    /// <param name="pedido">The active order representing the cart. If null or empty, returns an empty cart message.</param>
    /// <returns>A tuple containing the formatted Markdown text and the <see cref="InlineKeyboardMarkup"/>.</returns>
    public (string texto, InlineKeyboardMarkup markup) BuildUICarrito(Pedido? pedido)
    {
        if (pedido == null || !pedido.PedidoProductos.Any())
        {
            string msg = "🛒 *Tu carrito está vacío. Agrega productos para verlos aquí.*";
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

    /// <summary>
    /// Constructs the final order review UI, presenting the delivery details (address, phone, references)
    /// and a summary of the products before the user confirms and pays.
    /// </summary>
    /// <param name="pedido">The pending order with details like total and delivery address.</param>
    /// <param name="cliente">The client associated with the order.</param>
    /// <returns>A tuple containing the final summary text and the confirmation/correction buttons.</returns>
    public (string texto, InlineKeyboardMarkup markup) BuildUIResumenFinal(Pedido? pedido, Cliente? cliente)
    {
        var sb = new StringBuilder();
        if (cliente == null || pedido == null) return (sb.ToString(), null);
        if (cliente == null || pedido == null) return (sb.ToString(), null);
        sb.AppendLine("🏁 *VERIFICA TU PEDIDO*");
        sb.AppendLine("__________________________");
        var detallesMap = JsonSerializer.Deserialize<Dictionary<string, string>>(pedido.DetallesJson);
        if (detallesMap == null) detallesMap = new Dictionary<string, string>();

        PedidoDetalleDTO detalleDTO = new PedidoDetalleDTO
        {
            Referencias = detallesMap.GetValueOrDefault("Referencias", "Sin referencias"),
            Telefono = detallesMap.GetValueOrDefault("Telefono", "No proporcionado"),
            Email = detallesMap.GetValueOrDefault("Email", "No proporcionado")
        };
        
        foreach (var item in pedido.PedidoProductos)
        {
            sb.AppendLine($"▪️ {item.Producto.Nombre} x{item.Cantidad} — *${item.Cantidad * item.PrecioUnitario}*");
        }

        sb.AppendLine("__________________________");
        sb.AppendLine($"💰 *TOTAL A PAGAR: ${pedido.Total}*");
        sb.AppendLine("__________________________");
        
        sb.AppendLine($"📍 *Dirección de Envío:*`{pedido.DireccionEntrega}`");
        sb.AppendLine($"🔸 Referencias: `{detalleDTO.Referencias}`");
        sb.AppendLine($"📞 *Teléfono:* `{detalleDTO.Telefono}`");
        sb.AppendLine("__________________________\n");
        sb.AppendLine("¿Toda la información es correcta?");
        
        var buttons = new List<InlineKeyboardButton[]>
    {
        new[] {
            InlineKeyboardButton.WithCallbackData("🚀 CONFIRMAR Y PAGAR", "checkoutEnd"),
            InlineKeyboardButton.WithCallbackData("🚫 CANCELAR PEDIDO", "ask_clear"),
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

    public (string texto, InlineKeyboardMarkup markup) Ticket(PagosLinksDTO? pagosLinks, int pedidoId, string url)
    {
        var sb = new StringBuilder();
        var keyboard = new InlineKeyboardMarkup();
        if (pagosLinks == null || string.IsNullOrEmpty(pagosLinks.Url))
        {
            keyboard.AddButton(InlineKeyboardButton.WithCallbackData("🔄 Reintentar", "checkoutEnd"));
            keyboard.AddButton(InlineKeyboardButton.WithCallbackData("🛒 Volver al Carrito", "cart"));            
            return ("⚠️ No pudimos generar el pago. Inténtalo de nuevo", keyboard);
        }
            
        sb.AppendLine($"*¡ORDEN \\#{pedidoId} LISTA\\!*\n");
        sb.AppendLine($"*Referencia:* `{pagosLinks.Referencia}`");
        sb.AppendLine($"*Estado:* {pagosLinks.EstadoPago}\n");
        sb.AppendLine($"Haz clic en el botón de abajo para pagar");        
        
        keyboard.AddButton(InlineKeyboardButton.WithUrl("➡️ IR A PAGAR", url));
        keyboard.AddButton(InlineKeyboardButton.WithCallbackData("🛒 Volver al Carrito", "cart"));
        
        return (sb.ToString(), keyboard);
    }
    
}