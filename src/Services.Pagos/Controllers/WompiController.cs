using Microsoft.AspNetCore.Mvc;
using Services.Pagos.Models;
using Services.Pagos.Services;
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
                return BadRequest($"No se encontró el pedido {pedidoId}");

            var contenido = await response.Content.ReadAsStringAsync();
            var pagoDb = JsonSerializer.Deserialize<PagoDto>(contenido, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (pagoDb == null)
                return BadRequest("Datos inválidos desde Inventario");

            var montoFinal = pagoDb.Monto > 0 ? pagoDb.Monto : pagoDb.Total;

            var referencia = string.IsNullOrWhiteSpace(pagoDb.ReferenciaTransaccion)
                ? $"REF-{pedidoId}"   // 🔥 IMPORTANTE: usar mismo formato que Wompi devuelve
                : pagoDb.ReferenciaTransaccion;

            var solicitud = new WompiTransactionRequest
            {
                Monto = montoFinal,
                Referencia = referencia,
                RedirectUrl = _configuration["Wompi:RedirectUrl"] ?? ""
            };

            var resultado = await _wompiService.CrearEnlacePago(solicitud);

            if (!resultado.Success)
                return StatusCode(500, resultado.Error);

            return Ok(new
            {
                url = resultado.PaymentLink,
                referencia
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando enlace");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("webhook/wompi")]
    public async Task<IActionResult> RecibirWebhookWompi()
    {
        Request.EnableBuffering();

        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        _logger.LogInformation("🔥 WEBHOOK RAW: {Body}", rawBody);

        if (string.IsNullOrWhiteSpace(rawBody))
            return Ok();

        try
        {
            using var jsonDoc = JsonDocument.Parse(rawBody);
            var data = jsonDoc.RootElement;

            // 🔥 REFERENCIA (CORREGIDO PARA WOMPI SV)
            string? referencia = null;

            if (data.TryGetProperty("EnlacePago", out var enlace) &&
                enlace.TryGetProperty("IdentificadorEnlaceComercio", out var refAlt))
            {
                referencia = refAlt.GetString();
            }
            else if (data.TryGetProperty("IdExterno", out var idExt))
            {
                referencia = idExt.GetString();
            }

            // 🔥 ESTADO
            var estado = data.TryGetProperty("ResultadoTransaccion", out var estadoProp)
                ? estadoProp.GetString()
                : null;

            // 🔥 ID TRANSACCIÓN
            var idTransaccion = data.TryGetProperty("IdTransaccion", out var idProp)
                ? idProp.GetString()
                : null;

            // 🔥 MONTO (ARREGLADO)
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

            // 🔥 MÉTODO PAGO
            var metodo = data.TryGetProperty("FormaPagoUtilizada", out var metodoProp)
                ? metodoProp.GetString()
                : "Desconocido";

            _logger.LogInformation("📦 Ref: {Ref} | Estado: {Estado} | Monto: {Monto}",
                referencia, estado, monto);

            if (string.IsNullOrWhiteSpace(referencia))
                return Ok();

            if (!EsPagoExitoso(estado))
                return Ok();

            // 🔗 ACTUALIZAR INVENTARIO
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
                _logger.LogError("❌ Error Inventario: {Resp}", resp);
                return Ok();
            }

            _logger.LogInformation("✅ Pago actualizado: {Ref}", referencia);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error webhook");
            return Ok();
        }
    }

    private static bool EsPagoExitoso(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
            return false;

        return estado.Equals("ExitosaAprobada", StringComparison.OrdinalIgnoreCase)
            || estado.Contains("Aprobada", StringComparison.OrdinalIgnoreCase)
            || estado.Contains("Exitosa", StringComparison.OrdinalIgnoreCase);
    }

    public class PagoDto
    {
        public decimal Monto { get; set; }
        public decimal Total { get; set; }
        public string ReferenciaTransaccion { get; set; } = string.Empty;
    }
}