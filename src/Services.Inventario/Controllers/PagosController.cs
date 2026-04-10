using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Microsoft.AspNetCore.SignalR; // <-- Necesario para SignalR
using Service.Inventario.Hubs; // <-- Asegúrate de tener el namespace correcto para tu Hub

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/pagos")]
public class PagosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PagosController> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;


    public PagosController(ApplicationDbContext context, ILogger<PagosController> logger, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    // GET: api/pagos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pago>>> GetPagos()
    {
        // Incluimos Pedido por si necesitas mostrar datos del pedido en la tabla
        var pagos = await _context.Pagos
            .Include(p => p.Pedido)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

        return Ok(pagos);
    }

    // GET: api/pagos/{id}
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
    public async Task<ActionResult<Pago>> GetPagoPorPedido(int pedidoId)
    {
        var pago = await _context.Pagos
            .Include(p => p.Pedido)
            .FirstOrDefaultAsync(p => pedidoId == pedidoId);

        if (pago == null)
        {
            return NotFound(new { message = $"No se encontró un pago para el pedido {pedidoId}" });
        }

        return Ok(pago);
    }

    // POST: api/pagos
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

    //PUT: api/pagos/{id}

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
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPost("actualizar-por-referencia/{referencia?}")] // El '?' hace que sea opcional
    public async Task<IActionResult> UpdatePorReferencia(string? referencia)
    {
        // Si la referencia viene nula (porque Wompi no la mandó en la URL), 
        // le ponemos un texto por defecto
        var refFinal = referencia ?? "Webhook de Wompi";

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            titulo = "¡Wompi nos contactó!",
            mensaje = "Se recibió una señal de pago. Referencia: " + refFinal,
            tipo = "success",
            fecha = DateTime.Now
        });

        return Ok(new { mensaje = "Recibido correctamente" });
    }

}