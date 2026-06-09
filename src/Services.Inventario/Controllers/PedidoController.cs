using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Microsoft.AspNetCore.SignalR; 
using Service.Inventario.Hubs;      
using Services.Inventario.Validators;

namespace Services.Inventario.Controllers;

/// <summary>
/// API Controller for managing customer orders (pedidos).
/// </summary>
[ApiController]
[Route("api/inventario")]
public class PedidoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PedidoController> _logger;
    private readonly IHubContext<NotificationHub> _hubContext; // <-- Agregado

    /// <summary>
    /// Initializes a new instance of the <see cref="PedidoController"/> class.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="hubContext">The SignalR hub context for sending real-time notifications.</param>
    public PedidoController(
        ApplicationDbContext context, 
        ILogger<PedidoController> logger,
        IHubContext<NotificationHub> hubContext) 
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Retrieves all orders, ordered by creation date in descending order.
    /// </summary>
    /// <returns>A list of all orders.</returns>
    // GET: api/inventario/pedidos
    [HttpGet("pedidos")]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        var pedidos = await _context.Pedidos
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync();

        return Ok(pedidos);
    }

    /// <summary>
    /// Retrieves a paged result of orders with optional search filtering.
    /// </summary>
    /// <param name="page">The page number (defaults to 1).</param>
    /// <param name="pageSize">The number of items per page (defaults to 10).</param>
    /// <param name="search">A string to filter orders by ID, delivery address, or Wompi reference.</param>
    /// <returns>A paged result containing the requested orders.</returns>
    // GET: api/inventario/pedidos/paged
    [HttpGet("pedidos/paged")]
    public async Task<ActionResult<PagedResult<Pedido>>> GetPedidosPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
    {
        var query = _context.Pedidos.Include(p=>p.Cliente).Include(p=>p.Usuario).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Id.ToString().Contains(s) || 
                                    (p.DireccionEntrega != null && p.DireccionEntrega.ToLower().Contains(s)) ||
                                    (p.ReferenciaWompi != null && p.ReferenciaWompi.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreadoEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Pedido> { Items = items, TotalCount = total });
    }

    /// <summary>
    /// Retrieves a specific order by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the order to retrieve.</param>
    /// <returns>The requested order if found; otherwise, a 404 Not Found response.</returns>
    // GET: api/inventario/pedidos/{id}
    [HttpGet("pedidos/{id}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido no encontrado" });
        }

        return Ok(pedido);
    }

    /// <summary>
    /// Creates a new order and sets the server-side timestamps.
    /// Sends a real-time notification about the new order.
    /// </summary>
    /// <param name="pedido">The order object to be created.</param>
    /// <returns>The newly created order with its assigned ID.</returns>
    // POST: api/inventario/pedidos
    [HttpPost("pedidos")]
    public async Task<ActionResult<Pedido>> CreatePedido([FromBody] Pedido pedido)
    {
        pedido.CreadoEn = DateTime.UtcNow;
        pedido.ActualizadoEn = DateTime.UtcNow;

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pedido creado: {PedidoId}", pedido.Id);

        // Sends a real-time notification to all connected clients (e.g., dashboard)
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new 
        {
            titulo = "Nuevo pedido recibido",
            mensaje = $"Se ha generado el pedido #{pedido.Id} por ${pedido.Total}",
            tipo = "success", 
            fecha = DateTime.Now
        });

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

    /// <summary>
    /// Updates an existing order's details.
    /// Sends a real-time notification about the updated order.
    /// </summary>
    /// <param name="id">The ID of the order to update.</param>
    /// <param name="pedido">The order data to update.</param>
    /// <returns>A 204 No Content response on success, or an error status code.</returns>
    // PUT: api/inventario/pedidos/{id}
    [HttpPut("pedidos/{id}")]
    public async Task<IActionResult> UpdatePedido(int id, [FromBody] Pedido pedido)
    {
        if (id != pedido.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var pedidoExistente = await _context.Pedidos.FindAsync(id);
        if (pedidoExistente == null)
        {
            return NotFound(new { message = "Pedido no encontrado" });
        }

        pedidoExistente.UsuarioId = pedido.UsuarioId;
        pedidoExistente.ClienteId = pedido.ClienteId;
        pedidoExistente.Estado = pedido.Estado;
        pedidoExistente.Total = pedido.Total;
        pedidoExistente.DireccionEntrega = pedido.DireccionEntrega;
        pedidoExistente.DetallesJson = pedido.DetallesJson;
        pedidoExistente.ReferenciaWompi = pedido.ReferenciaWompi;
        pedidoExistente.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Pedido actualizado: {PedidoId}", id);

        // Sends a real-time notification about the order update
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new 
        {
            titulo = "Pedido Actualizado",
            mensaje = $"El pedido #{id} ahora está en estado: {pedido.Estado}",
            tipo = "info",
            fecha = DateTime.Now
        });

        return NoContent();
    }

    /// <summary>
    /// Permanently deletes an order from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the order to remove.</param>
    /// <returns>A 204 No Content response on success, or a 404 Not Found response if not found.</returns>
    // DELETE: api/inventario/pedidos/{id}
    [HttpDelete("pedidos/{id}")]
    public async Task<IActionResult> DeletePedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido no encontrado" });
        }

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pedido eliminado: {PedidoId}", id);

        return NoContent();
    }
}