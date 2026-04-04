using Microsoft.EntityFrameworkCore;
using Services.ChatBot.Interfaces;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Webhook.Controllers.Controllers;

public class SqlBotPersistence(ApplicationDbContext context) : IBotPersistencia
{
    public async Task<Conversacion?> ObtenerConversacionActiva(long clienteId)
    {
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

    public async Task<Pedido?> ObtenerPedidoActivo(long TelegramId)
    {
        return await context.Pedidos
        .Include(p => p.PedidoProductos)
        .ThenInclude(pp => pp.Producto)
        .FirstOrDefaultAsync(p => p.Cliente.TelegramId == TelegramId && p.Estado == EstadoPedido.Pendiente);
    }
}