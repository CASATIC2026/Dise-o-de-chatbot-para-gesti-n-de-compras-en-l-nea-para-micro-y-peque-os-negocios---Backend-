using Microsoft.AspNetCore.Mvc;
using Services.Pagos.Models;
using Services.Pagos.Services;
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
        var url = $"{inventarioBaseUrl.TrimEnd('/')}/api/pagos/pedido/{pedidoId}";

        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest($"No se encontro el pedido {pedidoId} en Inventario.");
            }

            var contenido = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var pagoDb = JsonSerializer.Deserialize<PagoDto>(contenido, options);

            if (pagoDb == null || (pagoDb.Monto <= 0 && pagoDb.Total <= 0))
            {
                return BadRequest("Monto invalido recibido del Inventario.");
            }

            var montoFinal = pagoDb.Monto > 0 ? pagoDb.Monto : pagoDb.Total;
            if (montoFinal <= 0)
            {
                return BadRequest("El pedido no tiene un monto valido para generar el enlace.");
            }

            var solicitudWompi = new WompiTransactionRequest
            {
                Monto = montoFinal,
                Referencia = $"PAGO-{pedidoId}-{DateTime.UtcNow.Ticks}",
                RedirectUrl = _configuration["Wompi:RedirectUrl"] ?? string.Empty
            };

            var resultado = await _wompiService.CrearEnlacePago(solicitudWompi);

            if (!resultado.Success)
            {
                return StatusCode(500, new { error = "Error Wompi", detalle = resultado.Error });
            }

            return Ok(new
            {
                url = resultado.PaymentLink,
                montoCobrado = montoFinal,
                referencia = solicitudWompi.Referencia
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando el enlace automatico para el pedido {PedidoId}", pedidoId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    public class PagoDto
    {
        public decimal Monto { get; set; }
        public decimal Total { get; set; }
        public string ReferenciaTransaccion { get; set; } = string.Empty;
    }
}
