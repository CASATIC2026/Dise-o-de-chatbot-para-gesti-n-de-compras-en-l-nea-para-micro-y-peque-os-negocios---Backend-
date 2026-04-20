using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Services.ChatBot.DTOs;
using Services.ChatBot.Interfaces;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Webhook.Controllers.Controllers;

/// <summary>
/// Provides SQL-based persistence operations for the chatbot, including conversation management,
/// message logging, client registration, and shopping cart operations.
/// </summary>
public class SqlBotPersistence(ApplicationDbContext context) : IBotPersistencia
{
    /// <summary>
    /// Retrieves the active conversation for a given client Telegram ID.
    /// </summary>
    /// <param name="clienteId">The Telegram ID of the client.</param>
    /// <returns>The active conversation if found, otherwise null.</returns>
    public async Task<Conversacion?> ObtenerConversacionActiva(long clienteId)
    {
        var cliente = await ObtenerCliente(clienteId);

        if (cliente == null) return null;
        return await context.Conversaciones.FirstOrDefaultAsync(c => c.ClienteId == cliente.Id && c.Activa == true);
    }

    /// <summary>
    /// Updates or creates an active conversation for the specified client.
    /// </summary>
    /// <param name="clienteId">The Telegram ID of the client.</param>
    /// <param name="messageId">The message ID to associate with the conversation.</param>
    /// <param name="activa">Whether the conversation should be active.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ActualizarConversacion(long clienteId, int messageId, bool activa)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == clienteId);
        var elder = await context.Conversaciones.Where(c => c.ClienteId == cliente!.Id && c.Activa).ToListAsync();
        // Deactivate all existing active conversations for the client to ensure only one active at a time
        foreach (var v in elder)
        {
            v.Activa = false;
            v.ActualizadoEn = DateTime.UtcNow;
        }
        var conv = await ObtenerConversacionActiva(cliente!.Id);

        if (conv == null)
        {
            conv = new Conversacion { ClienteId = (int)cliente!.Id, CreadoEn = DateTime.UtcNow };
            context.Conversaciones.Add(conv);
        }
        else { return; }// Only create a new conversation if none is active; do not update existing
        conv.Asunto = messageId.ToString();
        conv.Activa = true;
        conv.ActualizadoEn = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Registers a message in the conversation.
    /// </summary>
    /// <param name="conversacionId">The ID of the conversation.</param>
    /// <param name="contenido">The content of the message.</param>
    /// <param name="remitente">The type of sender.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RegistrarMensaje(int conversacionId, string contenido, TipoRemitente remitente)
    {
        context.Mensajes.Add(new Mensaje
        {
            ConversacionId = conversacionId,
            Contenido = contenido,
            Remitente = remitente,
            FechaEnvio = DateTime.UtcNow
        });

        var conv = await context.Conversaciones.FirstOrDefaultAsync(c => c.Id == conversacionId);
        if (conv == null) return;
        conv.ActualizadoEn = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }
    /// <summary>
    /// Registers a new client if not already exists.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="nombre">The name of the client.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RegistrarCliente(long TelegramId, string nombre)
    {
        var cliente = await ObtenerCliente(TelegramId);
        if (cliente == null)
        {
            context.Clientes.Add(
                new Cliente
                {
                    TelegramId = TelegramId,
                    Nombre = nombre ?? "Usuario Telegram",
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
        }
    }
    /// <summary>
    /// Adds a product to the client's cart.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="productoId">The ID of the product.</param>
    /// <param name="cantidad">The quantity to add.</param>
    /// <returns>A tuple indicating success and a message.</returns>
    public async Task<(bool Success, string msg)> AgregarProducto(long TelegramId, int productoId, int cantidad)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {

            var cliente = ObtenerCliente(TelegramId);
            if (cliente == null) return (false, "Cliente no encontrado. Escribe /start para iniciar una nueva compra");


            var producto = await context.Productos.FindAsync(productoId);
            if (producto == null || !producto.Activo) return (false, "Producto no disponible");
            if (producto.StockDisponible < cantidad || producto.StockDisponible <= 0) return (false, "Lo sentimos, no queda stock suficiente.");


            var pedido = await ObtenerPedidoActivo(TelegramId);

            if (pedido == null)
            {
                pedido = new Pedido
                {
                    ClienteId = cliente.Id,
                    Estado = EstadoPedido.Pendiente,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow,
                    Total = 0
                };
                context.Pedidos.Add(pedido);
                await context.SaveChangesAsync();
            }


            var item = pedido.PedidoProductos.FirstOrDefault(p => p.ProductoId == productoId);

            if (item == null)
            {
                item = new PedidoProducto
                {
                    PedidoId = pedido.Id,
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.Precio,
                    CreadoEn = DateTime.UtcNow

                };
                context.PedidoProductos.Add(item);
            }
            else
            {
                item.Cantidad += cantidad;
            }

            producto.StockReservado += cantidad;
            pedido.ActualizadoEn = DateTime.UtcNow;
            pedido.Total += producto.Precio * cantidad;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, $"{producto.Nombre} añadido al carrito");
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return (false, "Error al procesar la reserva" + e.Message);
        }
    }
    /// <summary>
    /// Updates the quantity of a product in the cart.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="productoId">The ID of the product.</param>
    /// <param name="cantidad">The new quantity.</param>
    /// <returns>A tuple indicating success and a message.</returns>
    public async Task<(bool Succes, string msg)> ActualizarCantidadCarrito(long TelegramId, int productoId, int cantidad)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var pedido = await ObtenerPedidoActivo(TelegramId);
            if (pedido == null) return (false, "No se encontró un carrito activo.");

            var item = pedido.PedidoProductos.FirstOrDefault(pp => pp.ProductoId == productoId);
            if (item == null) return (false, "El producto no está en el carrito.");

            var producto = await context.Productos.FindAsync(productoId);
            if (producto == null) return (false, "Producto no encontrado");

            int diferencia = cantidad - item.Cantidad;// Calculate the difference to adjust stock reservation
            if (diferencia > 0 && producto.StockDisponible < diferencia)
                return (false, $"Lo sentimos, no queda stock suficiente. Stock: {producto.StockDisponible}");

            producto.StockReservado += diferencia;
            item.Cantidad = cantidad;
            pedido.Total += pedido.PedidoProductos.Sum(pp => pp.Cantidad * pp.PrecioUnitario);
            pedido.ActualizadoEn = DateTime.UtcNow;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Cantidad actualizada");

        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error al actualizar cantidad: " + e.Message);
            await transaction.RollbackAsync();
            return (false, "Error al actualizar la cantidad");
        }
    }
    /// <summary>
    /// Retrieves the active order for the client.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <returns>The active order if found, otherwise null.</returns>
    public async Task<Pedido?> ObtenerPedidoActivo(long TelegramId)
    {
        return await context.Pedidos
        .Include(p => p.PedidoProductos)
        .ThenInclude(pp => pp.Producto)
        .FirstOrDefaultAsync(p => p.Cliente!.TelegramId == TelegramId && p.Estado == EstadoPedido.Pendiente);
    }
    /// <summary>
    /// Retrieves the client by Telegram ID.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <returns>The client if found, otherwise null.</returns>
    public async Task<Cliente?> ObtenerCliente(long TelegramId)
    {
        return await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == TelegramId);
    }
    /// <summary>
    /// Empties the client's cart.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <returns>True if successful, otherwise false.</returns>
    public async Task<bool> VaciarCarrito(long TelegramId)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var pedido = await ObtenerPedidoActivo(TelegramId);
            if (pedido == null || !pedido.PedidoProductos.Any()) return false;
            foreach (var pp in pedido.PedidoProductos)
            {
                var producto = await context.Productos.FindAsync(pp.ProductoId);
                if (producto != null) producto.StockReservado -= pp.Cantidad;// Release reserved stock for each item
            }

            pedido.Estado = EstadoPedido.Cancelado;
            pedido.Total = 0;
            pedido.ActualizadoEn = DateTime.UtcNow;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error al vaciar carrito: " + e.Message);
            await transaction.RollbackAsync();
            return false;
        }
    }
    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="productoId">The ID of the product.</param>
    /// <returns>A tuple indicating success and a message.</returns>
    public async Task<(bool Succes, string msg)> EliminarItem(long TelegramId, int productoId)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var pedido = await ObtenerPedidoActivo(TelegramId);
            if (pedido == null) return (false, "No se encontró un carrito activo.");

            var item = pedido.PedidoProductos.FirstOrDefault(pp => pp.ProductoId == productoId);
            if (item == null) return (false, "El producto no está en el carrito.");

            var producto = await context.Productos.FindAsync(item.ProductoId);

            if (producto != null) producto.StockReservado -= item.Cantidad;// Release the reserved stock for the removed item

            pedido.Total -= item.PrecioUnitario * item.Cantidad;// Subtract the item's total from the order total
            pedido.ActualizadoEn = DateTime.UtcNow;

            context.PedidoProductos.Remove(item);
            Console.WriteLine($"\nproductoId al eliminar: {item.ProductoId}\n");
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Producto eliminado del carrito");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error al eliminar item: " + e.Message);
            await transaction.RollbackAsync();
            return (false, "Error al eliminar el item");
        }
    }
    /// <summary>
    /// Updates the client's information.
    /// </summary>
    /// <param name="dtoC">The client DTO with updated information.</param>
    /// <returns>True if updated successfully, otherwise false.</returns>
    public async Task<bool> ActualizarCliente(ClienteDTO dtoC)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == dtoC.TelegramId);
        if (cliente == null) return false;

        cliente.Nombre = dtoC.Nombre ?? cliente.Nombre;
        cliente.Direccion = dtoC.Direccion ?? cliente.Direccion;
        cliente.Telefono = dtoC.Telefono ?? cliente.Telefono;
        cliente.Email = dtoC.Email ?? cliente.Email;

        cliente.ActualizadoEn = DateTime.UtcNow;

        return await context.SaveChangesAsync() > 0;
    }
    /// <summary>
    /// Retrieves the user's orders with pagination.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="tamaño">The page size.</param>
    /// <param name="pagina">The page number.</param>
    /// <returns>A tuple with the list of orders and the total count.</returns>

    public async Task<(List<Pedido>, int count)> ObtenerPedidosUsuario(long TelegramId, int tamaño, int pagina)
    {
        var pedidosUsuario = await context.Pedidos
        .Include(p => p.PedidoProductos)
        .ThenInclude(pp => pp.Producto)
        .Where(p => p.Cliente!.TelegramId == TelegramId && p.Estado != EstadoPedido.Cancelado)
        .OrderByDescending(p => p.CreadoEn)
        .Skip(pagina * tamaño)
        .Take(tamaño)
        .ToListAsync();

        var count = await context.Pedidos
        .Include(p => p.PedidoProductos)
        .Where(p => p.Cliente!.TelegramId == TelegramId && p.Estado != EstadoPedido.Cancelado)
        .CountAsync();
        if (pedidosUsuario == null || pedidosUsuario.Count == 0) return (new List<Pedido>(), 0);
        return (pedidosUsuario, count);
    }
    /// <summary>
    /// Updates the active order with new details.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="pdd">The order DTO with updates.</param>
    /// <returns>A tuple indicating success and a message.</returns>

    public async Task<(bool Succes, string msg)> ActualizarPedido(long TelegramId, PedidoDTO pdd)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var pedido = await ObtenerPedidoActivo(TelegramId);
            if (pedido == null)
            {
                return (false, "No se encontró un carrito activo.");
            }

            pedido.Estado = pdd.Estado;
            Console.WriteLine("Total:" + pdd.Total);
            if (pdd.Total != null)
                pedido.Total = (decimal)pdd.Total;

            pedido.ActualizadoEn = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(pdd.Direccion))
            {
                pedido.DireccionEntrega = pdd.Direccion;
            }

            Dictionary<string, string> detallesMap;

            if (string.IsNullOrWhiteSpace(pedido.DetallesJson) || pedido.DetallesJson == "[]")
            {
                detallesMap = new Dictionary<string, string>();
            }
            else
            {
                detallesMap = JsonSerializer.Deserialize<Dictionary<string, string>>(pedido.DetallesJson)
                              ?? new Dictionary<string, string>();
            }

            if (pdd.Detalles != null)
            {
                if (!string.IsNullOrEmpty(pdd.Detalles.Referencias))
                    detallesMap["Referencias"] = pdd.Detalles.Referencias;

                if (!string.IsNullOrEmpty(pdd.Detalles.Telefono))
                    detallesMap["Telefono"] = pdd.Detalles.Telefono;

                if (!string.IsNullOrEmpty(pdd.Detalles.Email))
                    detallesMap["Email"] = pdd.Detalles.Email;
            }

            pedido.DetallesJson = JsonSerializer.Serialize(detallesMap); // Serialize additional details as JSON for flexible storage

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Pedido #{pedido.Id} actualizado con éxito.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.Error.WriteLine($"[Error ActualizarPedido]: {ex.Message}");
            return (false, "Error interno al finalizar el pedido.");
        }
    }
}