using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Service.Inventario.Hubs;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/pagos")]
public class PagosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PagosController> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public PagosController(
        ApplicationDbContext context,
        ILogger<PagosController> logger,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pago>>> GetPagos()
    {
        var pagos = await _context.Pagos
            .Include(p => p.Pedido)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

        return Ok(pagos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Pago>> GetPago(int id)
    {
        var pago = await _context.Pagos
            .Include(p => p.Pedido)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pago == null)
        {
            return NotFound(new { message = "Pago no encontrado" });
        }

        return Ok(pago);
    }

    [HttpGet("pedido/{pedidoId}")]
    public async Task<IActionResult> GetPagoPorPedido(int pedidoId)
    {
        var pago = await _context.Pagos
            .Include(p => p.Pedido)
            .Where(p => p.PedidoId == pedidoId)
            .OrderByDescending(p => p.CreadoEn)
            .FirstOrDefaultAsync();

        if (pago != null)
        {
            return Ok(new
            {
                pago.Id,
                pago.PedidoId,
                pago.Monto,
                Total = pago.Pedido?.Total ?? pago.Monto,
                pago.ReferenciaTransaccion
            });
        }

        var pedido = await _context.Pedidos.FindAsync(pedidoId);

        if (pedido == null)
        {
            return NotFound(new { message = $"No se encontro un pago ni pedido para el pedido {pedidoId}" });
        }

        return Ok(new
        {
            Id = 0,
            PedidoId = pedido.Id,
            Monto = pedido.Total,
            Total = pedido.Total,
            ReferenciaTransaccion = pedido.ReferenciaWompi ?? $"PED-{pedido.Id}"
        });
    }

    [HttpPost]
    public async Task<ActionResult<Pago>> CreatePago([FromBody] Pago pago)
    {
        pago.FechaPago = DateTime.UtcNow;
        pago.CreadoEn = DateTime.UtcNow;
        pago.ActualizadoEn = DateTime.UtcNow;

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pago registrado: {PagoId} para el Pedido: {PedidoId}", pago.Id, pago.PedidoId);

        return CreatedAtAction(nameof(GetPago), new { id = pago.Id }, pago);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePago(int id, [FromBody] Pago pago)
    {
        if (id != pago.Id)
        {
            return BadRequest(new { messaje = "El ID no coincide" });
        }

        pago.ActualizadoEn = DateTime.UtcNow;
        _context.Entry(pago).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Pagos.Any(e => e.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpPost("actualizar-por-referencia/{referencia?}")]
    public async Task<IActionResult> UpdatePorReferencia(string? referencia)
    {
        var refFinal = referencia ?? "Webhook de Wompi";

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            titulo = "Wompi nos contacto",
            mensaje = "Se recibio una senal de pago. Referencia: " + refFinal,
            tipo = "success",
            fecha = DateTime.Now
        });

        return Ok(new { mensaje = "Recibido correctamente" });
    }
}
