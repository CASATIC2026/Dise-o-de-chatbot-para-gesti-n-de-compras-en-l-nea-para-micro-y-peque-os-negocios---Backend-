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

        var pedido = pago?.Pedido ?? await _context.Pedidos.FindAsync(pedidoId);

        if (pedido == null)
        {
            return NotFound(new { message = $"No se encontro un pago ni pedido para el pedido {pedidoId}" });
        }

        var fechaActual = DateTime.UtcNow;

        if (pago == null)
        {
            var referenciaInicial = pedido.ReferenciaWompi ?? CreateReference(pedido.Id);
            pago = new Pago
            {
                PedidoId = pedido.Id,
                Monto = pedido.Total,
                MetodoPago = "WOMPI",
                Estado = EstadoPago.Pendiente,
                ReferenciaTransaccion = referenciaInicial,
                FechaPago = fechaActual,
                CreadoEn = fechaActual,
                ActualizadoEn = fechaActual
            };

            pedido.Pago = pago;
            pedido.ReferenciaWompi = referenciaInicial;
            pedido.ActualizadoEn = fechaActual;
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            return Ok(BuildPagoPedidoResponse(pago, pedido));
        }

        if (pedido.Estado == EstadoPedido.Cancelado || pago.Estado == EstadoPago.Cancelado)
        {
            return BadRequest(new { message = $"El pedido {pedidoId} esta cancelado y no puede generar enlaces de pago." });
        }

        if (pedido.Estado == EstadoPedido.Pagado || pago.Estado == EstadoPago.Completado)
        {
            var nuevaReferencia = CreateReference(pedido.Id);
            pedido.Estado = EstadoPedido.Pendiente;
            pedido.ReferenciaWompi = nuevaReferencia;
            pedido.ActualizadoEn = fechaActual;

            pago.Estado = EstadoPago.Pendiente;
            pago.Monto = pedido.Total;
            pago.MetodoPago = string.IsNullOrWhiteSpace(pago.MetodoPago) ? "WOMPI" : pago.MetodoPago;
            pago.ReferenciaTransaccion = nuevaReferencia;
            pago.FechaPago = fechaActual;
            pago.ActualizadoEn = fechaActual;

            await _context.SaveChangesAsync();
            return Ok(BuildPagoPedidoResponse(pago, pedido));
        }

        if (string.IsNullOrWhiteSpace(pago.ReferenciaTransaccion))
        {
            pago.ReferenciaTransaccion = pedido.ReferenciaWompi ?? CreateReference(pedido.Id);
            pago.ActualizadoEn = fechaActual;
        }

        if (string.IsNullOrWhiteSpace(pedido.ReferenciaWompi))
        {
            pedido.ReferenciaWompi = pago.ReferenciaTransaccion;
            pedido.ActualizadoEn = fechaActual;
        }

        pago.Estado = EstadoPago.Pendiente;
        pago.Monto = pedido.Total;
        pago.MetodoPago = string.IsNullOrWhiteSpace(pago.MetodoPago) ? "WOMPI" : pago.MetodoPago;
        pago.ActualizadoEn = fechaActual;

        if (pedido.Estado != EstadoPedido.Pendiente)
        {
            pedido.Estado = EstadoPedido.Pendiente;
            pedido.ActualizadoEn = fechaActual;
        }

        await _context.SaveChangesAsync();

        return Ok(BuildPagoPedidoResponse(pago, pedido));
    }

    [HttpPost]
    public async Task<ActionResult<Pago>> CreatePago([FromBody] Pago pago)
    {
        var pedidoExiste = await _context.Pedidos.AnyAsync(p => p.Id == pago.PedidoId);
        if (!pedidoExiste)
        {
            return BadRequest(new { message = $"El pedido {pago.PedidoId} no existe." });
        }

        var fechaActual = DateTime.UtcNow;
        pago.FechaPago = pago.FechaPago == default ? fechaActual : pago.FechaPago;
        pago.CreadoEn = fechaActual;
        pago.ActualizadoEn = fechaActual;

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
            return BadRequest(new { message = "El ID no coincide" });
        }

        var pagoExistente = await _context.Pagos.FirstOrDefaultAsync(p => p.Id == id);
        if (pagoExistente == null)
        {
            return NotFound(new { message = "Pago no encontrado" });
        }

        var pedidoExiste = await _context.Pedidos.AnyAsync(p => p.Id == pago.PedidoId);
        if (!pedidoExiste)
        {
            return BadRequest(new { message = $"El pedido {pago.PedidoId} no existe." });
        }

        pagoExistente.PedidoId = pago.PedidoId;
        pagoExistente.Monto = pago.Monto;
        pagoExistente.MetodoPago = pago.MetodoPago;
        pagoExistente.Estado = pago.Estado;
        pagoExistente.ReferenciaTransaccion = pago.ReferenciaTransaccion;
        pagoExistente.FechaPago = pago.FechaPago == default ? pagoExistente.FechaPago : pago.FechaPago;
        pagoExistente.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePago(int id)
    {
        var pago = await _context.Pagos.FindAsync(id);
        if (pago == null)
        {
            return NotFound(new { message = "Pago no encontrado" });
        }

        _context.Pagos.Remove(pago);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("actualizar-por-referencia/{referencia?}")]
    public async Task<IActionResult> UpdatePorReferencia(
        string? referencia,
        [FromBody] ActualizarPagoPorReferenciaRequest? request)
    {
        var refFinal = string.IsNullOrWhiteSpace(referencia)
            ? request?.ReferenciaTransaccion
            : referencia;

        if (string.IsNullOrWhiteSpace(refFinal))
        {
            return BadRequest(new { mensaje = "Debe enviar una referencia valida." });
        }

        var pago = await _context.Pagos
            .Include(p => p.Pedido)
            .OrderByDescending(p => p.CreadoEn)
            .FirstOrDefaultAsync(p =>
                p.ReferenciaTransaccion == refFinal ||
                (p.Pedido != null && p.Pedido.ReferenciaWompi == refFinal));

        var pedido = pago?.Pedido;

        if (pedido == null)
        {
            pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.ReferenciaWompi == refFinal);
        }

        if (pedido == null && TryExtractPedidoId(refFinal, out var pedidoId))
        {
            pedido = await _context.Pedidos.FindAsync(pedidoId);
        }

        if (pedido == null)
        {
            _logger.LogWarning("No se encontro pedido para la referencia {Referencia}", refFinal);
            return NotFound(new { mensaje = $"No se encontro un pedido asociado a la referencia {refFinal}" });
        }

        var fechaActual = DateTime.UtcNow;

        if ((pago != null && pago.Estado == EstadoPago.Completado) || pedido.Estado == EstadoPedido.Pagado)
        {
            _logger.LogWarning("Se intento procesar un pago duplicado para la referencia {Referencia}", refFinal);
            return Conflict(new
            {
                mensaje = "El enlace ya fue utilizado y el pedido ya se encuentra pagado.",
                pedidoId = pedido.Id,
                referencia = refFinal,
                estadoPedido = pedido.Estado.ToString(),
                estadoPago = pago?.Estado.ToString() ?? EstadoPago.Completado.ToString()
            });
        }

        if (pago == null)
        {
            pago = new Pago
            {
                PedidoId = pedido.Id,
                Monto = request?.Monto > 0 ? request.Monto.Value : pedido.Total,
                MetodoPago = string.IsNullOrWhiteSpace(request?.MetodoPago) ? "WOMPI" : request!.MetodoPago!,
                Estado = EstadoPago.Completado,
                ReferenciaTransaccion = refFinal,
                FechaPago = fechaActual,
                CreadoEn = fechaActual,
                ActualizadoEn = fechaActual
            };

            _context.Pagos.Add(pago);
        }
        else
        {
            pago.Estado = EstadoPago.Completado;
            pago.Monto = request?.Monto > 0 ? request.Monto.Value : pago.Monto;
            pago.MetodoPago = string.IsNullOrWhiteSpace(request?.MetodoPago) ? pago.MetodoPago : request!.MetodoPago!;
            pago.ReferenciaTransaccion = refFinal;
            pago.FechaPago = fechaActual;
            pago.ActualizadoEn = fechaActual;
        }

        pedido.ReferenciaWompi = refFinal;
        pedido.Estado = EstadoPedido.Pagado;
        pedido.ActualizadoEn = fechaActual;

        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            titulo = "Pago confirmado por Wompi",
            mensaje = $"El pedido #{pedido.Id} cambio a Pagado. Referencia: {refFinal}",
            tipo = "success",
            fecha = DateTime.Now
        });

        return Ok(new
        {
            mensaje = "Pago actualizado correctamente",
            pedidoId = pedido.Id,
            referencia = refFinal,
            estadoPedido = pedido.Estado.ToString(),
            estadoPago = pago.Estado.ToString()
        });
    }

    private static object BuildPagoPedidoResponse(Pago pago, Pedido? pedido) => new
    {
        pago.Id,
        pago.PedidoId,
        pago.Monto,
        Total = pedido?.Total ?? pago.Monto,
        pago.ReferenciaTransaccion,
        EstadoPago = (int)pago.Estado,
        EstadoPedido = pedido != null ? (int)pedido.Estado : 0
    };

    private static string CreateReference(int pedidoId)
    {
        return $"PED-{pedidoId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static bool TryExtractPedidoId(string referencia, out int pedidoId)
    {
        pedidoId = 0;

        var partes = referencia.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (partes.Length < 2)
        {
            return false;
        }

        return int.TryParse(partes[1], out pedidoId);
    }

    public class ActualizarPagoPorReferenciaRequest
    {
        public string? ReferenciaTransaccion { get; set; }
        public string? IdTransaccion { get; set; }
        public decimal? Monto { get; set; }
        public string? MetodoPago { get; set; }
        public string? ResultadoTransaccion { get; set; }
        public bool? EsProductiva { get; set; }
    }
}
