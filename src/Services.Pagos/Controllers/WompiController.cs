using Microsoft.AspNetCore.Mvc;
using Services.Pagos.Services;
using Shared.Core.Entities;
using Shared.Core.Entities;
using System.Text;
using System.Text.Json;

namespace Services.Pagos.Controllers;

[ApiController]
[Route("api/pagos")]
public class WompiController : ControllerBase
{
    private readonly WompiService _wompiService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WompiController> _logger;

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

    [HttpPost("crear-enlace-automatico/{pedidoId}")]
    public async Task<IActionResult> CrearEnlaceAutomatico(int pedidoId)
    {
        var inventarioBaseUrl = _configuration["Services:InventarioBaseUrl"] ?? "http://localhost:5041";
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
                    referencia = referencia,
                    estado = EstadoPedido.Pagado
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

    public class PagoDto
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public decimal Monto { get; set; }
        public decimal Total { get; set; }
        public string ReferenciaTransaccion { get; set; } = string.Empty;
        public int EstadoPago { get; set; }
        public int EstadoPedido { get; set; }
    }
}
