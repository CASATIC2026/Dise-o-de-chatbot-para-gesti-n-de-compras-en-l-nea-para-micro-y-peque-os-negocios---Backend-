using Microsoft.EntityFrameworkCore;
using Services.ChatBot.DTOs;
using Services.ChatBot.Interfaces;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Webhook.Controllers.Controllers;

public class SqlBotPersistence(ApplicationDbContext context) : IBotPersistencia
{
    public async Task<Conversacion?> ObtenerConversacionActiva(long clienteId)
    {
        Console.WriteLine($"Conversacion , TelegramId {clienteId}");
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == clienteId);

        if (cliente == null) return null;
        return await context.Conversaciones.FirstOrDefaultAsync(c => c.ClienteId == cliente.Id && c.Activa == true);
    }

    public async Task ActualizarConversacion(long clienteId, int messageId, bool activa)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == clienteId);
        var elder = await context.Conversaciones.Where(c => c.ClienteId == cliente!.Id && c.Activa).ToListAsync();

        foreach (var v in elder)
        {
            v.Activa = false;
            v.ActualizadoEn = DateTime.UtcNow;
        }

        //var conv = await context.Conversaciones.FirstOrDefaultAsync(c => c.ClienteId == cliente!.Id);
        var conv = await ObtenerConversacionActiva(cliente!.Id);
        Console.WriteLine("conversacion", conv);
        if (conv == null)
        {
            conv = new Conversacion { ClienteId = (int)cliente!.Id, CreadoEn = DateTime.UtcNow };
            context.Conversaciones.Add(conv);
        }
        else { return; }
        conv.Asunto = messageId.ToString();
        conv.Activa = true;
        conv.ActualizadoEn = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task RegistrarMensaje(int conversacionId, string contenido, TipoRemitente remitente)
    {
        context.Mensajes.Add(new Mensaje
        {
            ConversacionId = conversacionId,
            Contenido = contenido,
            Remitente = remitente,
            FechaEnvio = DateTime.UtcNow
        });
        context.Conversaciones.FirstOrDefault(c => c.Id == conversacionId).ActualizadoEn = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task RegistrarCliente(long TelegramId, string nombre)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == TelegramId);
        //Console.WriteLine("s"+cliente.TelegramId);
        if (cliente == null)
        {
            Console.WriteLine("s" + TelegramId + " " + nombre);
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

    public async Task<(bool Success, string msg)> AgregarProducto(long TelegramId, int productoId, int cantidad)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            //Verificar cliente en base
            var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == TelegramId);
            if (cliente == null) return (false, "Cliente no encontrado. Escribe /start para iniciar una nueva compra");

            //Verificar disponibilidad de stock
            var producto = await context.Productos.FindAsync(productoId);
            if (producto == null || !producto.Activo) return (false, "Producto no disponible");
            if (producto.StockDisponible < cantidad || producto.StockDisponible <= 0) return (false, "Lo sentimos, no queda stock suficiente.");

            //Verificar si existen pedidos
            var pedido = await context.Pedidos
            .Include(p => p.PedidoProductos)
            .FirstOrDefaultAsync(p => p.ClienteId == cliente.Id
            && p.Estado == EstadoPedido.Pendiente);

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

            //Gestion de Carrito en PedidoProducto
            var item = pedido.PedidoProductos.FirstOrDefault(p => p.ProductoId == productoId);

            if (item == null) //Validar la cantidad de entrada!
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
            pedido.Total += (producto.Precio * cantidad); //Actualizar al descartar producto!

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

            int diferencia = cantidad - item.Cantidad;
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

    public async Task<Pedido?> ObtenerPedidoActivo(long TelegramId)
    {
        return await context.Pedidos
        .Include(p => p.PedidoProductos)
        .ThenInclude(pp => pp.Producto)
        .FirstOrDefaultAsync(p => p.Cliente!.TelegramId == TelegramId && p.Estado == EstadoPedido.Pendiente);
    }
    public async Task<Cliente?> ObtenerCliente(long TelegramId)
    {
        return await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == TelegramId);
    }

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
                if (producto != null) producto.StockReservado -= pp.Cantidad;
            }
            //context.PedidoProductos.RemoveRange(pedido.PedidoProductos);
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

            if (producto != null) producto.StockReservado -= item.Cantidad;

            pedido.Total -= (item.PrecioUnitario * item.Cantidad);
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

    public async Task<(List<Pedido>, int count)> ObtenerPedidosUsuario(long TelegramId, int tamaño, int pagina)
    {
        var pedidosUsuario = await context.Pedidos
        .Include(p => p.PedidoProductos)
        .ThenInclude(pp => pp.Producto)
        .Where(p => p.Cliente!.TelegramId == TelegramId && p.Estado != EstadoPedido.Cancelado)
        .OrderBy(p => p.CreadoEn)
        .Skip(pagina * tamaño)
        .Take(tamaño)
        .ToListAsync();

        var count = await context.Pedidos
        .Include(p => p.PedidoProductos)
        .Where(p => p.Cliente!.TelegramId == TelegramId && p.Estado != EstadoPedido.Cancelado)
        .CountAsync();
        if (pedidosUsuario == null || pedidosUsuario.Count == 0) return (null, 0);
        return (pedidosUsuario, count);
    }
}