using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/inventario")]
public class PedidoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PedidoController> _logger;

    public PedidoController(ApplicationDbContext context, ILogger<PedidoController> logger)
    {
        _context = context;
        _logger = logger;
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
