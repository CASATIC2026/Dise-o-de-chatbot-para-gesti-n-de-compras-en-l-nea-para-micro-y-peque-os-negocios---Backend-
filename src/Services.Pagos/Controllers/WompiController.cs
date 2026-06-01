using Microsoft.AspNetCore.Mvc;
using Services.Pagos.Services;
using Shared.Core.Entities;
using Shared.Core.Entities;
using System.Text;
using System.Text.Json;

namespace Services.Pagos.Controllers;

/// <summary>
/// Controller for managing Wompi payment gateway integration, including link generation and webhook processing.
/// </summary>
[ApiController]
[Route("api/pagos")]
public class WompiController : ControllerBase
{
    private readonly WompiService _wompiService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WompiController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WompiController"/> class.
    /// </summary>
    /// <param name="wompiService">The service for interacting with Wompi API.</param>
    /// <param name="httpClientFactory">The factory to create HTTP clients for internal service communication.</param>
    /// <param name="configuration">The system configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public WompiController(
        WompiService wompiService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WompiController> logger)
    {
        _wompiService = wompiService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Automatically creates a Wompi payment link for a specific order.
    /// It fetches order data from the inventory service and validates states before requesting the link.
    /// </summary>
    /// <param name="pedidoId">The ID of the order to pay.</param>
    /// <returns>An <see cref="IActionResult"/> containing the payment URL and reference.</returns>
    [HttpPost("crear-enlace-automatico/{pedidoId}")]
    public async Task<IActionResult> CrearEnlaceAutomatico(int pedidoId)
    {
        var inventarioBaseUrl = _configuration["Services:InventarioBaseUrl"] ?? "http://inventario-service:8080";
        using var client = _httpClientFactory.CreateClient();

        try
        {
            var response = await client.GetAsync($"{inventarioBaseUrl}/api/pagos/pedido/{pedidoId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, string.IsNullOrWhiteSpace(error) ? $"No se pudo preparar el pedido {pedidoId}" : error);
            }

            var contenido = await response.Content.ReadAsStringAsync();
            var pagoDb = JsonSerializer.Deserialize<PagoDto>(contenido, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (pagoDb == null)
            {
                return BadRequest("Datos invalidos desde Inventario");
            }

            var validacionEstado = ValidarEstadosParaCrearEnlace(pedidoId, pagoDb);
            if (validacionEstado != null)
            {
                return Conflict(validacionEstado);
            }

            var montoFinal = pagoDb.Monto > 0 ? pagoDb.Monto : pagoDb.Total;
            var montoMaximoPorEnlace = _configuration.GetValue<decimal?>("Wompi:MaxAmountPerLink") ?? 1000m;

            if (montoFinal > montoMaximoPorEnlace)
            {
                return BadRequest(new
                {
                    message = $"El pedido {pedidoId} tiene un total de ${montoFinal:F2} y Wompi solo permite hasta ${montoMaximoPorEnlace:F2} por enlace.",
                    pedidoId,
                    monto = montoFinal,
                    montoMaximo = montoMaximoPorEnlace
                });
            }

            var referencia = string.IsNullOrWhiteSpace(pagoDb.ReferenciaTransaccion)
                ? $"PED-{pedidoId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}"
                : pagoDb.ReferenciaTransaccion;

            var solicitud = new Models.WompiTransactionRequest
            {
                Monto = montoFinal,
                Referencia = referencia,
                RedirectUrl = _configuration["Wompi:RedirectUrl"] ?? string.Empty
            };

            var resultado = await _wompiService.CrearEnlacePago(solicitud);
            if (!resultado.Success)
            {
                return StatusCode(500, resultado.Error);
            }

            return Ok(new
            {
                url = resultado.PaymentLink,
                referencia,
                estadoPedido = pagoDb.EstadoPedido,
                estadoPago = pagoDb.EstadoPago
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando enlace de pago para el pedido {PedidoId}", pedidoId);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Webhook endpoint called by Wompi to notify the system of transaction results.
    /// It updates the payment and order status and notifies the chatbot service.
    /// </summary>
    /// <returns>A 200 OK response to acknowledge receipt of the webhook.</returns>
    [HttpPost("webhook/wompi")]
    public async Task<IActionResult> RecibirWebhookWompi()
    {
        Console.WriteLine("Entro al webhook wompi");
        Request.EnableBuffering();

        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;
        var chatBotBaseUrl = _configuration["Services:ChatBotBaseUrl"] ?? "http://chatbot-service:8080";
        
        _logger.LogInformation("WEBHOOK RAW: {Body}", rawBody);

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return Ok();
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(rawBody);
            var data = jsonDoc.RootElement;

            string? referencia = null;
            long? enlaceId = null;

            if (data.TryGetProperty("EnlacePago", out var enlace))
            {
                if (enlace.TryGetProperty("IdentificadorEnlaceComercio", out var refAlt))
                {
                    referencia = refAlt.GetString();
                }

                if (enlace.TryGetProperty("Id", out var enlaceIdProp) &&
                    enlaceIdProp.ValueKind == JsonValueKind.Number &&
                    enlaceIdProp.TryGetInt64(out var enlaceIdValue))
                {
                    enlaceId = enlaceIdValue;
                }
            }
            else if (data.TryGetProperty("IdExterno", out var idExt))
            {
                referencia = idExt.GetString();
            }

            var estado = data.TryGetProperty("ResultadoTransaccion", out var estadoProp)
                ? estadoProp.GetString()
                : null;

            var idTransaccion = data.TryGetProperty("IdTransaccion", out var idProp)
                ? idProp.GetString()
                : null;

            decimal monto = 0;
            if (data.TryGetProperty("Monto", out var montoProp))
            {
                if (montoProp.ValueKind == JsonValueKind.String &&
                    decimal.TryParse(montoProp.GetString(), out var parsed))
                {
                    monto = parsed;
                }
                else if (montoProp.ValueKind == JsonValueKind.Number)
                {
                    monto = montoProp.GetDecimal();
                }
            }

            var metodo = data.TryGetProperty("FormaPagoUtilizada", out var metodoProp)
                ? metodoProp.GetString()
                : "Desconocido";

            _logger.LogInformation(
                "Webhook Wompi recibido. Ref: {Ref} | Estado: {Estado} | Monto: {Monto} | EnlaceId: {EnlaceId}",
                referencia,
                estado,
                monto,
                enlaceId);

            if (string.IsNullOrWhiteSpace(referencia) || !EsPagoExitoso(estado))
            {
                return Ok();
            }

            var inventarioBaseUrl = _configuration["Services:InventarioBaseUrl"] ?? "http://inventario-service:8080";
            using var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"{inventarioBaseUrl}/api/pagos/actualizar-por-referencia/{Uri.EscapeDataString(referencia)}",
                new
                {
                    referenciaTransaccion = referencia,
                    idTransaccion,
                    monto,
                    metodoPago = metodo,
                    resultadoTransaccion = estado,
                    esProductiva = true
                });

            var resp = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error Inventario al actualizar pago {Ref}: {Resp}", referencia, resp);
                return Ok();
            }

            if (enlaceId.HasValue)
            {
                var enlaceDesactivado = await _wompiService.DesactivarEnlacePago(enlaceId.Value);
                if (!enlaceDesactivado)
                {
                    _logger.LogWarning("No se logro desactivar el enlace Wompi {EnlaceId} luego del pago exitoso.", enlaceId.Value);
                }
            }
            else
            {
                _logger.LogWarning("Webhook exitoso sin Id de enlace Wompi para la referencia {Ref}", referencia);
            }


            try
            {
                await client.PostAsJsonAsync($"{chatBotBaseUrl}/api/bot/pagos-completado", new
                {
                    referencia,
                    estado = EstadoPedido.Pagado,
                    url = ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo realizar la notificacion al servicio de chat bot.");
            }
            _logger.LogInformation("Pago actualizado y enlace invalidado para la referencia {Ref}", referencia);
            return Ok();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando webhook de Wompi");
            return Ok();
        }
    }
    [HttpPost("")]

    /// <summary>
    /// Determines if a transaction result status string from Wompi represents a successful payment.
    /// </summary>
    /// <param name="estado">The status string provided by Wompi.</param>
    /// <returns>True if the status indicates success; otherwise, false.</returns>
    private static bool EsPagoExitoso(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            return false;
        }

        return estado.Equals("ExitosaAprobada", StringComparison.OrdinalIgnoreCase)
            || estado.Contains("Aprobada", StringComparison.OrdinalIgnoreCase)
            || estado.Contains("Exitosa", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if the current states of the order and payment allow for the creation of a new payment link.
    /// </summary>
    /// <param name="pedidoId">The order ID.</param>
    /// <param name="pago">The current payment data transferred from the inventory service.</param>
    /// <returns>An error object if the state is invalid; otherwise, null.</returns>
    private static object? ValidarEstadosParaCrearEnlace(int pedidoId, PagoDto pago)
    {
        const int estadoPedidoPagado = 2;
        const int estadoPedidoCancelado = 4;
        const int estadoPagoCompletado = 2;
        const int estadoPagoCancelado = 4;

        if (pago.EstadoPedido == estadoPedidoCancelado)
        {
            return new
            {
                message = $"El pedido {pedidoId} esta cancelado y no puede generar enlaces de pago.",
                pedidoId,
                estadoPedido = pago.EstadoPedido,
                estadoPago = pago.EstadoPago
            };
        }

        if (pago.EstadoPedido == estadoPedidoPagado)
        {
            return new
            {
                message = $"El pedido {pedidoId} ya esta pagado y no puede generar nuevos enlaces de pago.",
                pedidoId,
                estadoPedido = pago.EstadoPedido,
                estadoPago = pago.EstadoPago
            };
        }

        if (pago.EstadoPago == estadoPagoCompletado)
        {
            return new
            {
                message = $"El pago del pedido {pedidoId} ya esta completado y no puede generar nuevos enlaces de pago.",
                pedidoId,
                estadoPedido = pago.EstadoPedido,
                estadoPago = pago.EstadoPago
            };
        }

        if (pago.EstadoPago == estadoPagoCancelado)
        {
            return new
            {
                message = $"El pago del pedido {pedidoId} esta cancelado y no puede generar enlaces de pago.",
                pedidoId,
                estadoPedido = pago.EstadoPedido,
                estadoPago = pago.EstadoPago
            };
        }

        return null;
    }

    /// <summary>
    /// Internal Data Transfer Object representing payment and order status info returned from the inventory service.
    /// </summary>
    public class PagoDto
    {
        /// <summary>The payment ID.</summary>
        public int Id { get; set; }
        /// <summary>The associated order ID.</summary>
        public int PedidoId { get; set; }
        /// <summary>The amount specifically associated with the payment record.</summary>
        public decimal Monto { get; set; }
        /// <summary>The total amount for the order.</summary>
        public decimal Total { get; set; }
        /// <summary>The transaction reference used for tracking.</summary>
        public string ReferenciaTransaccion { get; set; } = string.Empty;
        /// <summary>The current state of the payment.</summary>
        public int EstadoPago { get; set; }
        /// <summary>The current state of the order.</summary>
        public int EstadoPedido { get; set; }
    }
}
