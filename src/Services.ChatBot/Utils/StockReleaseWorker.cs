using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.ChatBot.Utils;

/// <summary>
/// Background service that periodically checks for expired pending orders and releases the reserved stock.
/// This ensures that products in abandoned carts are made available to other customers again.
/// </summary>
public class StockReleaseWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scoperFactory;
    private readonly ILogger<StockReleaseWorker> _logger;

    /// <summary>
    /// Time in minutes before a pending order is considered expired and its stock is released.
    /// </summary>
    private const int TIEMPO_EXPIRACION = 15;
        //Nt: cambiar el tiempo a insertado por usuario.
    public StockReleaseWorker(IServiceScopeFactory scopeFactory, ILogger<StockReleaseWorker> logger)
    {
        _scoperFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background cleanup task. It runs every 60 seconds to process orders
    /// that have exceeded the <see cref="TIEMPO_EXPIRACION"/> limit.
    /// </summary>
    /// <param name="stoppingToken">Triggered when the host is shutting down.</param>
    /// <returns>A task that represents the background operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Limpieza de Stock start");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using(var scope = _scoperFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Calculate the cutoff time for expiration
                    var limiteTiempo = DateTime.UtcNow.AddMinutes(-TIEMPO_EXPIRACION);

                    // Find pending orders that haven't been updated recently
                    var pedidosExpirados = await context.Pedidos
                    .Include(p => p.PedidoProductos)
                    .Where(p => p.Estado == EstadoPedido.Pendiente && p.ActualizadoEn < limiteTiempo)
                    .ToListAsync();

                    if (pedidosExpirados.Any())
                    {
                        _logger.LogInformation($"Procesando {pedidosExpirados.Count()} pedidos expirados ...");

                        foreach(var pedido in pedidosExpirados)
                        {
                            // Revert the reserved stock for each product in the order
                            foreach(var item in pedido.PedidoProductos)
                            {
                                var producto = await context.Productos.FindAsync(item.ProductoId);

                                if(producto != null)
                                {
                                    producto.StockReservado -= item.Cantidad;
                                    if (producto.StockReservado < 0) producto.StockReservado = 0;
                                }
                            }

                            // Mark the order as canceled so it isn't processed again
                            pedido.Estado = EstadoPedido.Cancelado;
                            pedido.ActualizadoEn = DateTime.UtcNow;
                        }                        
                        await context.SaveChangesAsync();
                        _logger.LogInformation("Limpieza de Stock end");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error en Limpieza de Stock");
            }
            
            // Wait for 60 seconds before the next check
            //nt. dar opcion de cambio de tiempo por variable de entorno
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}