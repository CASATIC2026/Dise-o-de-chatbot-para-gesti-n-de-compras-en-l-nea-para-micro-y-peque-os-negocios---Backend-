using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Microsoft.AspNetCore.SignalR; // <-- Agregado
using Service.Inventario.Hubs;      // <-- Agregado (Asegúrate que el namespace coincida con tu NotificationHub)
using Services.Inventario.Validators;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/inventario")]
public class PedidoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PedidoController> _logger;
    private readonly IHubContext<NotificationHub> _hubContext; // <-- Agregado

    // Constructor actualizado con la inyección del Hub
    public PedidoController(
        ApplicationDbContext context, 
        ILogger<PedidoController> logger,
        IHubContext<NotificationHub> hubContext) 
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    // GET: api/inventario/pedidos
    [HttpGet("pedidos")]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        var pedidos = await _context.Pedidos
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync();

        return Ok(pedidos);
    }

    // GET: api/inventario/pedidos/paged
    [HttpGet("pedidos/paged")]
    public async Task<ActionResult<PagedResult<Pedido>>> GetPedidosPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
    {
        var query = _context.Pedidos.AsQueryable();

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

    // POST: api/inventario/pedidos
    [HttpPost("pedidos")]
    public async Task<ActionResult<Pedido>> CreatePedido([FromBody] Pedido pedido)
    {
        pedido.CreadoEn = DateTime.UtcNow;
        pedido.ActualizadoEn = DateTime.UtcNow;

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pedido creado: {PedidoId}", pedido.Id);

        // --- NOTIFICACIÓN EN TIEMPO REAL ---
        // Este objeto es el que recibirá React para actualizar la "Actividad Reciente"
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new 
        {
            titulo = "Nuevo pedido recibido",
            mensaje = $"Se ha generado el pedido #{pedido.Id} por ${pedido.Total}",
            tipo = "success", 
            fecha = DateTime.Now
        });

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

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

        // Notificación opcional cuando se actualiza el estado (ej. de pendiente a confirmado)
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new 
        {
            titulo = "Pedido Actualizado",
            mensaje = $"El pedido #{id} ahora está en estado: {pedido.Estado}",
            tipo = "info",
            fecha = DateTime.Now
        });

        return NoContent();
    }

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