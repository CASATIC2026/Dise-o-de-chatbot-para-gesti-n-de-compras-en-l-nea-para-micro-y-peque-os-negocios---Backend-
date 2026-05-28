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
                //InlineKeyboardButton.WithCallbackData("✅ Finalizar Compra", "checkout")                
                //InlineKeyboardButton.WithCallbackData("✅ Finalizar Compra", "checkout")
                InlineKeyboardButton.WithCallbackData("✅ Finalizar Compra", "Dep_0")
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
        var Direccion = pedido.DireccionEntrega.Trim().Replace("|", "\n- ");
        sb.AppendLine($"📍 *Dirección de Envío:*`\n{Direccion}`");
        sb.AppendLine($"🔸 Referencias: `{detalleDTO.Referencias}`");
        sb.AppendLine($"📞 *Teléfono:* `{detalleDTO.Telefono}`");
        sb.AppendLine("__________________________\n");
        sb.AppendLine("¿Toda la información es correcta?");
        
        var buttons = new List<InlineKeyboardButton[]>
    {
        new[] {
            InlineKeyboardButton.WithCallbackData("🚀 CONFIRMAR Y PAGAR", "checkoutEnd"),
            InlineKeyboardButton.WithCallbackData("🚫 CANCELAR", "ask_clear"),
        },
        new[] { 
            // Si algo está mal, lo regresamos al inicio para sobrescribir la data
            InlineKeyboardButton.WithCallbackData("🔄 Corregir Datos", $"Dep_{0}")
        },
        new[] {
            InlineKeyboardButton.WithCallbackData("🛒 Volver al Carrito", "cart")
        }
    };
        return (sb.ToString(), new InlineKeyboardMarkup(buttons));
    }

    /// <summary>
    /// Constructs the payment ticket UI, providing the order status, reference, 
    /// and the button to navigate to the external payment gateway.
    /// </summary>
    /// <param name="pagosLinks">The DTO containing payment link information and transaction reference.</param>
    /// <param name="pedidoId">The unique identifier of the order.</param>
    /// <param name="url">The secure redirection URL for the payment process.</param>
    /// <returns>A tuple containing the formatted Markdown text and the <see cref="InlineKeyboardMarkup"/> with the payment button.</returns>
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

    /// <summary>
    /// Builds the user interface for displaying a paginated list of departments.
    /// </summary>
    /// <param name="departamentos">A list of department names.</param>
    /// <param name="page">The current page number for pagination.</param>
    /// <returns>A tuple containing the formatted message text and the <see cref="InlineKeyboardMarkup"/> for department navigation.</returns>
    public (string texto, InlineKeyboardMarkup markup) Deptos(List<String> departamentos, int page)
    {
        var sb = new StringBuilder();
        var keyboard = new InlineKeyboardMarkup();
        var buttons = new List<InlineKeyboardButton[]>();
        List<String> deptos = new List<String>();
        if (departamentos == null || departamentos.Any())
        {
            int rango =  6;
            int start = 0;
            var longCuenta = departamentos!.LongCount().ToString();
            int cuenta = int.Parse(longCuenta);
            
            if(page != 0)
            {
                if((page + 1) * rango > cuenta)
                {
                    start = cuenta - Math.Abs((page * rango) - cuenta); 
                    
                    rango = cuenta;
                }else{
                    start = page * rango;
                    rango += start;
                }
                Console.WriteLine($"Cuenta: {cuenta}, Start: {start}, Rango: {rango}" );
            }
            for (int i = start; i < rango; i++)
            {
                deptos.Add(departamentos![i]);
                //keyboard.AddButton(InlineKeyboardButton.WithCallbackData(departamentos![i].ToString(), departamentos[i].ToString()));
            }    
            buttons = deptos.Select(d => 
                new [] {InlineKeyboardButton.WithCallbackData(d.ToString(), $"Muni_{d.ToString()}_{page}_{-1}")}).ToList();       

            var navRow = new List<InlineKeyboardButton>();
            if (page > 0) navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"Dep_{page - 1}"));
            if ((page + 1) *  6 < departamentos!.LongCount()) navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"Dep_{page + 1}"));
            if (navRow.Count != 0)  buttons.Add(navRow.ToArray());

            keyboard = new InlineKeyboardMarkup(buttons);
            sb.Append("*Seleccione Departamento*");
            return (sb.ToString(), keyboard);            
        }
        return (sb.ToString(), keyboard);
    }

    /// <summary>
    /// Builds the user interface for displaying a paginated list of municipalities within a specific department.
    /// </summary>
    /// <param name="municipios">A list of municipality names.</param>
    /// <param name="page">The current page number for pagination of municipalities.</param>
    /// <param name="pageDepto">The page number of the department list, used for navigation back to departments.</param>
    /// <param name="departamento">The name of the department to which the municipalities belong.</param>
    /// <returns>A tuple containing the formatted message text and the <see cref="InlineKeyboardMarkup"/> for municipality navigation.</returns>
    public (string texto, InlineKeyboardMarkup markup) Municipios(List<String> municipios, int page, int pageDepto, string departamento)
    {
        var sb = new StringBuilder();
        var keyboard = new InlineKeyboardMarkup();
        var buttons = new List<InlineKeyboardButton[]>();
        List<String> munis = new List<String>();
        if (municipios == null || municipios.Any())
        {
            int rango =  6;
            int start = 0;
            var longCuenta = municipios!.LongCount().ToString();
            int cuenta = int.Parse(longCuenta);
            
            if(page != 0)
            {
                if((page + 1) * rango > cuenta)
                {
                    start = cuenta - Math.Abs((page  * rango) - cuenta); 
                    rango = cuenta;
                }else{
                    start = page * rango;
                    rango += start;
                }
            }
            
            for (int i = start; i < rango; i++)
            {
                //buttons.Add([InlineKeyboardButton.WithCallbackData(municipios![i].ToString(), municipios[i].ToString())]);
                munis.Add(municipios![i]);
            }            
            buttons = munis.Select(d => 
                new [] {InlineKeyboardButton.WithCallbackData(d.ToString(), $"checkout_{d.ToString()}")}).ToList();     

            var navRow = new List<InlineKeyboardButton>();
            if (page > 0) navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"Muni_{departamento}_{pageDepto}_{page - 1}"));
            if ((page + 1) *  6 < municipios!.LongCount()) navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"Muni_{departamento}_{pageDepto}_{page + 1}"));
            if (navRow.Count != 0) buttons.Add(navRow.ToArray());
            buttons.Add([InlineKeyboardButton.WithCallbackData("🔙 Volver", $"Dep_{pageDepto}")]);

            sb.Append("*Seleccione Distrito/Municipio*");
            keyboard = new InlineKeyboardMarkup(buttons); 
            return (sb.ToString(), keyboard);            
        }
        return (sb.ToString(), keyboard);
    }
}