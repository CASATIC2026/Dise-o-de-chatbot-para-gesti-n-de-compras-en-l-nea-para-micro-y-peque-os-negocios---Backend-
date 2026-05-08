using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Service.Inventario.Hubs;
using Shared.Core.Data;
using Shared.Core.Entities;
using Services.Inventario.Validators;

namespace Services.Inventario.Controllers;

/// <summary>
/// API Controller for managing payments and transaction status within the inventory system.
/// </summary>
[ApiController]
[Route("api/pagos")]
public class PagosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PagosController> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagosController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="hubContext">The SignalR hub context for real-time notifications.</param>
    public PagosController(
        ApplicationDbContext context,
        ILogger<PagosController> logger,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Retrieves all payment records including their associated order information.
    /// </summary>
    /// <returns>A list of payments ordered by date descending.</returns>
    // GET: api/pagos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pago>>> GetPagos()
    {
        var pagos = await _context.Pagos
            .Include(p => p.Pedido)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

        return Ok(pagos);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<Pago>>> GetPagosPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
    {
        var query = _context.Pagos.Include(p => p.Pedido).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.ReferenciaTransaccion.ToLower().Contains(s) || 
                                    p.MetodoPago.ToLower().Contains(s) ||
                                    p.PedidoId.ToString().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.FechaPago)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Pago> { Items = items, TotalCount = total });
    }

    /// <summary>
    /// Retrieves a specific payment record by its unique identifier.
    /// </summary>
    /// <param name="id">The payment ID.</param>
    /// <returns>The requested payment if found; otherwise, a 404 Not Found response.</returns>
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

        if (pedido.Estado == EstadoPedido.Cancelado)
        {
            return Conflict(new { message = $"El pedido {pedidoId} esta cancelado y no puede generar enlaces de pago." });
        }

        if (pedido.Estado == EstadoPedido.Pagado)
        {
            return Conflict(new { message = $"El pedido {pedidoId} ya esta pagado y no puede generar nuevos enlaces de pago." });
        }

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

        if (pago.Estado == EstadoPago.Cancelado)
        {
            return Conflict(new { message = $"El pago del pedido {pedidoId} esta cancelado y no puede generar enlaces de pago." });
        }

        if (pago.Estado == EstadoPago.Completado)
        {
            return Conflict(new { message = $"El pago del pedido {pedidoId} ya esta completado y no puede generar nuevos enlaces de pago." });
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

    [HttpPost("marcar-rechazado/{referencia}")]
    public async Task<IActionResult> MarcarRechazadoPorTimeout(string referencia, [FromBody] MarcarRechazadoRequest? request)
    {
        if (string.IsNullOrWhiteSpace(referencia))
        {
            return BadRequest(new { mensaje = "Debe enviar una referencia valida." });
        }

        var pago = await _context.Pagos
            .Include(p => p.Pedido)
            .OrderByDescending(p => p.CreadoEn)
            .FirstOrDefaultAsync(p =>
                p.ReferenciaTransaccion == referencia ||
                (p.Pedido != null && p.Pedido.ReferenciaWompi == referencia));

        if (pago == null)
        {
            return NotFound(new { mensaje = $"No se encontro un pago para la referencia {referencia}" });
        }

        if (pago.Estado == EstadoPago.Completado)
        {
            return Conflict(new { mensaje = "El pago ya fue completado y no puede marcarse como rechazado.", referencia });
        }

        if (pago.Estado == EstadoPago.Rechazado)
        {
            return Ok(new { mensaje = "El pago ya estaba rechazado.", referencia });
        }

        var fechaActual = DateTime.UtcNow;
        pago.Estado = EstadoPago.Rechazado;
        pago.ActualizadoEn = fechaActual;

        if (pago.Pedido != null && pago.Pedido.Estado != EstadoPedido.Cancelado)
        {
            pago.Pedido.Estado = EstadoPedido.Cancelado;
            pago.Pedido.ActualizadoEn = fechaActual;
        }

        await _context.SaveChangesAsync();

        var motivo = string.IsNullOrWhiteSpace(request?.Motivo) ? "Timeout sin confirmacion de Wompi" : request!.Motivo!;
        _logger.LogWarning("Pago rechazado por timeout. Referencia: {Referencia}. Motivo: {Motivo}", referencia, motivo);

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            titulo = "Pago rechazado por timeout",
            mensaje = $"El pago con referencia {referencia} fue rechazado por tiempo excedido.",
            tipo = "warning",
            fecha = DateTime.Now
        });

        return Ok(new
        {
            mensaje = "Pago marcado como rechazado por timeout",
            referencia,
            estadoPago = pago.Estado.ToString(),
            estadoPedido = pago.Pedido?.Estado.ToString()
        });
    }

    /// <summary>
    /// Internal helper to build a consistent response object for payment/order status.
    /// </summary>
    /// <param name="pago">The payment entity.</param>
    /// <param name="pedido">The associated order entity.</param>
    /// <returns>An anonymous object for JSON response.</returns>
    private object BuildPagoPedidoResponse(Pago pago, Pedido pedido)
{
    return new
    {
        id = pago.Id,
        pedidoId = pedido.Id,
        monto = pago.Monto,
        total = pedido.Total,
        referenciaTransaccion = pago.ReferenciaTransaccion,
        estadoPago = (int)pago.Estado,
        estadoPedido = (int)pedido.Estado
    };
}

    /// <summary>
    /// Generates a unique transaction reference for Wompi.
    /// </summary>
    /// <param name="pedidoId">The order identifier.</param>
    /// <returns>A string reference in the format PED-{id}-{timestamp}.</returns>
    private static string CreateReference(int pedidoId)
    {
        return $"PED-{pedidoId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    /// <summary>
    /// Attempts to extract the Order ID from a standard transaction reference string.
    /// </summary>
    /// <param name="referencia">The reference string.</param>
    /// <param name="pedidoId">The outputted order identifier.</param>
    /// <returns>True if extraction was successful; otherwise, false.</returns>
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
    

    /// <summary>
    /// Request model for updating a payment via reference.
    /// </summary>
    public class ActualizarPagoPorReferenciaRequest
    {
        /// <summary>The transaction reference.</summary>
        public string? ReferenciaTransaccion { get; set; }
        /// <summary>The gateway-provided transaction ID.</summary>
        public string? IdTransaccion { get; set; }
        /// <summary>The confirmed payment amount.</summary>
        public decimal? Monto { get; set; }
        /// <summary>The payment method used.</summary>
        public string? MetodoPago { get; set; }
        /// <summary>The transaction result status from the gateway.</summary>
        public string? ResultadoTransaccion { get; set; }
        /// <summary>Indicates if the transaction was processed in a production environment.</summary>
        public bool? EsProductiva { get; set; }
    }

    /// <summary>
    /// Request model for marking a payment as rejected.
    /// </summary>
    public class MarcarRechazadoRequest
    {
        /// <summary>The reason for the rejection (e.g., Timeout).</summary>
        public string? Motivo { get; set; }
    }
}
