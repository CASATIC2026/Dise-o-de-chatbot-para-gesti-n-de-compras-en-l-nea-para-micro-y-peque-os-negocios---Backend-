using Microsoft.AspNetCore.Mvc;
using Services.Pagos.Services; // Asegúrate de que este sea el namespace de tu WompiService
using Services.Pagos.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Services.Pagos.Models; // Para encontrar WompiEnlaceRequest

namespace Services.Pagos.Controllers;

[ApiController]
[Route("api/pagos")]
public class WompiController : ControllerBase
{
    // EL TRUCO: Debes declarar estas variables aquí arriba
    private readonly WompiService _wompiService;
    private readonly IHttpClientFactory _httpClientFactory;

    public WompiController(WompiService wompiService, IHttpClientFactory httpClientFactory)
    {
        _wompiService = wompiService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("crear-enlace-automatico/{pedidoId}")]
    public async Task<IActionResult> CrearEnlaceAutomatico(int pedidoId)
    {
        using var client = new HttpClient();
        // IMPORTANTE: Puerto 8080 interno de Docker
        var url = $"http://inventario-service:8080/api/pagos/pedido/{pedidoId}";

        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return BadRequest($"No se encontró el pedido {pedidoId} en Inventario.");

            var contenido = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var pagoDb = JsonSerializer.Deserialize<PagoDto>(contenido, options);

            if (pagoDb == null || (pagoDb.Monto <= 0 && pagoDb.Total <= 0))
                return BadRequest("Monto inválido recibido del Inventario.");

            // Usamos el monto que venga (monto o total)
            decimal montoFinal = pagoDb.Monto > 0 ? pagoDb.Monto : pagoDb.Total;

            var solicitudWompi = new WompiTransactionRequest
            {
                Monto = montoFinal,
                Referencia = $"PAGO-{pedidoId}-{DateTime.Now.Ticks}", // Referencia dinámica
                RedirectUrl = "https://tu-sitio.com/confirmacion"
            };

            var resultado = await _wompiService.CrearEnlacePago(solicitudWompi);

            if (!resultado.Success)
                return StatusCode(500, new { error = "Error Wompi", detalle = resultado.Error });

            return Ok(new
            {
                url = resultado.PaymentLink,
                montoCobrado = montoFinal,
                referencia = solicitudWompi.Referencia
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }


    public class PagoDto
    {
        public decimal Monto { get; set; }
        public decimal Total { get; set; } // Por si el JSON viene con 'total'
        public string ReferenciaTransaccion { get; set; } = string.Empty;
    }

}