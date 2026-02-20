using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Services.Pagos.Services;

namespace Services.Pagos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly WompiService _wompiService;
    private readonly ILogger<PagosController> _logger;
    private readonly IConfiguration _configuration;

    public PagosController(
        ApplicationDbContext context, 
        WompiService wompiService, 
        ILogger<PagosController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _wompiService = wompiService;
        _logger = logger;
        _configuration = configuration;
    }

    // POST: api/pagos/iniciar
    [HttpPost("iniciar")]
    public async Task<ActionResult> IniciarPago([FromBody] IniciarPagoRequest request)
    {
        // Find the order
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == request.PedidoId);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido no encontrado" });
        }

        if (pedido.Estado != EstadoPedido.Pendiente)
        {
            return BadRequest(new { message = "El pedido ya ha sido procesado" });
        }

        // Create Wompi transaction
        var wompiRequest = new WompiTransactionRequest
        {
            Monto = pedido.Total,
            Email = request.Email,
            Referencia = $"PEDIDO-{pedido.Id}-{Guid.NewGuid().ToString().Substring(0, 8)}",
            RedirectUrl = request.RedirectUrl ?? _configuration["Wompi:DefaultRedirectUrl"] ?? "https://yourdomain.com/payment-confirmed"
        };

        var resultado = await _wompiService.CrearTransaccion(wompiRequest);

        if (!resultado.Success)
        {
            _logger.LogError("Error creating payment for order {PedidoId}: {Error}", 
                request.PedidoId, resultado.Error);
            
            return BadRequest(new { message = "Error al crear el pago", error = resultado.Error });
        }

        // Update order with Wompi reference
        pedido.ReferenciaWompi = resultado.TransactionId;
        pedido.Estado = EstadoPedido.Confirmado;
        pedido.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Payment initiated for order {PedidoId}, Wompi transaction: {TransactionId}", 
            request.PedidoId, resultado.TransactionId);

        return Ok(new
        {
            message = "Pago iniciado exitosamente",
            pedidoId = pedido.Id,
            transactionId = resultado.TransactionId,
            paymentLink = resultado.PaymentLink,
            referencia = wompiRequest.Referencia
        });
    }

    // POST: api/pagos/webhook
    [HttpPost("webhook")]
    public async Task<IActionResult> WebhookWompi([FromBody] WompiWebhookPayload payload)
    {
        try
        {
            _logger.LogInformation("Wompi webhook received: {EventType} - {TransactionId}", 
                payload.Event, payload.Data?.Transaction?.Id);

            // Validate webhook signature (implement this in production!)
            // var signature = Request.Headers["X-Wompi-Signature"];
            // if (!ValidateSignature(signature, payload)) return Unauthorized();

            if (payload.Event == "transaction.updated" && payload.Data?.Transaction != null)
            {
                var transactionId = payload.Data.Transaction.Id;
                var status = payload.Data.Transaction.Status;

                // Find order by Wompi reference
                var pedido = await _context.Pedidos
                    .FirstOrDefaultAsync(p => p.ReferenciaWompi == transactionId);

                if (pedido == null)
                {
                    _logger.LogWarning("Order not found for Wompi transaction: {TransactionId}", transactionId);
                    return Ok(new { message = "Order not found, webhook acknowledged" });
                }

                // Update order status based on payment status
                switch (status?.ToUpper())
                {
                    case "APPROVED":
                        pedido.Estado = EstadoPedido.Pagado;
                        _logger.LogInformation("Order {PedidoId} marked as PAID", pedido.Id);
                        break;
                    case "DECLINED":
                    case "ERROR":
                        pedido.Estado = EstadoPedido.Cancelado;
                        _logger.LogInformation("Order {PedidoId} marked as CANCELLED due to payment failure", pedido.Id);
                        break;
                    default:
                        _logger.LogInformation("Order {PedidoId} status unchanged, payment status: {Status}", 
                            pedido.Id, status);
                        break;
                }

                pedido.ActualizadoEn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Wompi webhook");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // GET: api/pagos/estado/{pedidoId}
    [HttpGet("estado/{pedidoId}")]
    public async Task<ActionResult> ConsultarEstadoPago(int pedidoId)
    {
        var pedido = await _context.Pedidos.FindAsync(pedidoId);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido no encontrado" });
        }

        if (string.IsNullOrEmpty(pedido.ReferenciaWompi))
        {
            return Ok(new
            {
                pedidoId = pedido.Id,
                estado = pedido.Estado.ToString(),
                message = "No se ha iniciado el pago para este pedido"
            });
        }

        // Query Wompi for transaction status
        var statusWompi = await _wompiService.ConsultarTransaccion(pedido.ReferenciaWompi);

        return Ok(new
        {
            pedidoId = pedido.Id,
            estado = pedido.Estado.ToString(),
            transactionId = pedido.ReferenciaWompi,
            wompiStatus = statusWompi.Status,
            monto = statusWompi.AmountInCents / 100m
        });
    }
}

// Request Models
public class IniciarPagoRequest
{
    public int PedidoId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }
}

// Webhook Models
public class WompiWebhookPayload
{
    public string Event { get; set; } = string.Empty;
    public WompiWebhookData? Data { get; set; }
}

public class WompiWebhookData
{
    public WompiWebhookTransaction? Transaction { get; set; }
}

public class WompiWebhookTransaction
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}
