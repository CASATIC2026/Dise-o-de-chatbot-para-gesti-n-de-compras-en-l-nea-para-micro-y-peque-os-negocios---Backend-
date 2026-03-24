using Microsoft.AspNetCore.Mvc;
using Services.Pagos.Services; // Asegúrate de que este sea el namespace de tu WompiService
using Services.Pagos.Models;
using System.Net.Http.Json;

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

    [HttpPost("crear-enlace")]
    public async Task<IActionResult> CrearEnlace([FromBody] WompiTransactionRequest request)
    {
        var resultado = await _wompiService.CrearEnlacePago(request);

        if (!resultado.Success)
        {
            return BadRequest(new { message = "Error al crear enlace", error = resultado.Error });
        }

        return Ok(new 
        { 
            url = resultado.PaymentLink, 
            referencia = request.Referencia,
            idWompi = resultado.TransactionId 
        });
    }

    [HttpPost("webhook/wompi")]
    public async Task<IActionResult> ProcesarNotificacionWompi([FromBody] WompiWebhookRequest request)
    {
        if (request == null) return BadRequest();

        // 1. Solo procesar si fue aprobado
        if (request.Estado.ToUpper() == "APPROVED")
        {
            var cliente = _httpClientFactory.CreateClient();

            // 2. Llamada interna al microservicio de Inventario (Puerto interno 8080)
            var response = await cliente.PutAsync(
                $"http://inventario-service:8080/api/pagos/actualizar-por-referencia/{request.Referencia}", 
                null
            );

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Dashboard actualizado" });
            }
        }

        return Ok(); // Wompi requiere 200 siempre
    }
}