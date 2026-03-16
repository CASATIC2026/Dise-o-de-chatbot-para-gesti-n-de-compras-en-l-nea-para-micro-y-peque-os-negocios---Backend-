using Microsoft.AspNetCore.Mvc;
using Services.Pagos.Services;
using Services.Pagos.Models;

namespace Services.Pagos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagosController : ControllerBase
{
    private readonly WompiService _wompiService;
    private readonly ILogger<PagosController> _logger;

    public PagosController(WompiService wompiService, ILogger<PagosController> logger)
    {
        _wompiService = wompiService;
        _logger = logger;
    }

    // NUEVO ENDPOINT PARA PRUEBAS QUEMADAS
    // POST: api/pagos/prueba-enlace
    [HttpPost("prueba-enlace")]
    public async Task<IActionResult> PruebaEnlace()
    {
        _logger.LogInformation("Iniciando prueba de enlace quemado para Wompi SV");

        // DATOS QUEMADOS
        var wompiRequest = new WompiTransactionRequest
        {
            Monto = 1.00m, // $1.00 USD
            Referencia = $"TEST-{Guid.NewGuid().ToString().Substring(0, 8)}",
            RedirectUrl = "https://google.com" 
        };

        var resultado = await _wompiService.CrearEnlacePago(wompiRequest);

        if (!resultado.Success)
        {
            return BadRequest(new { 
                message = "Error en la comunicación con Wompi SV", 
                detalles = resultado.Error 
            });
        }

        return Ok(new
        {
            message = "¡Conexión exitosa con Wompi El Salvador!",
            urlPago = resultado.PaymentLink,
            idWompi = resultado.TransactionId,
            referenciaUsada = wompiRequest.Referencia
        });
    }
}