using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Inventario.Controllers; 

[ApiController]
[Route("api/pagos")] 
public class PagosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
        private readonly ILogger<PagosController> _logger;

    public PagosController(ApplicationDbContext context, ILogger<PagosController> logger)
    {
        _context = context;
        _logger = logger;
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
}