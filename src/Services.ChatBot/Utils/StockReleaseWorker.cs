using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.ChatBot.Utils;

public class StockReleaseWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scoperFactory;
    private readonly ILogger<StockReleaseWorker> _logger;
    private const int TIEMPO_EXPIRACION = 15;
        //Nt: cambiar el tiempo a insertado por usuario.
    public StockReleaseWorker(IServiceScopeFactory scopeFactory, ILogger<StockReleaseWorker> logger)
    {
        _scoperFactory = scopeFactory;
        _logger = logger;
    }

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

                    var limiteTiempo = DateTime.UtcNow.AddMinutes(-TIEMPO_EXPIRACION);

                    var pedidosExpirados = await context.Pedidos
                    .Include(p => p.PedidoProductos)
                    .Where(p => p.Estado == EstadoPedido.Pendiente && p.ActualizadoEn < limiteTiempo)
                    .ToListAsync();

                    if (pedidosExpirados.Any())
                    {
                        _logger.LogInformation($"Procesando {pedidosExpirados.Count()} pedidos expirados ...");

                        foreach(var pedido in pedidosExpirados)
                        {
                            foreach(var item in pedido.PedidoProductos)
                            {
                                var producto = await context.Productos.FindAsync(item.ProductoId);

                                if(producto != null)
                                {
                                    producto.StockReservado -= item.Cantidad;
                                    if (producto.StockReservado < 0) producto.StockReservado = 0;
                                }
                            }
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
            
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}