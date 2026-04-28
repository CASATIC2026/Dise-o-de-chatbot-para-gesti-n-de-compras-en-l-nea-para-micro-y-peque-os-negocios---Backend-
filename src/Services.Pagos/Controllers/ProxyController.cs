using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Pagos.Controllers;

[ApiController]
[Route("api/pagos")]
public class ProxyController(ILogger<ProxyController> logger, ApplicationDbContext context) : ControllerBase
{
    private readonly ILogger<ProxyController> _logger = logger;
    private readonly ApplicationDbContext _context = context;

    [HttpGet("redirect")]
    public async Task<IActionResult> RastrearRedirigir(
    [FromQuery] string url,
    [FromQuery] string convasacionId
    )
    {
        Console.WriteLine("Url en metodo de redirect:" + url + "idConv: " + convasacionId);
        try
        {
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(convasacionId))
            {
                //Console.WriteLine("Url en metodo de redirect" + url);
                var conv = await _context.Conversaciones.FirstOrDefaultAsync(c => c.Asunto == convasacionId.ToString());
                var msg = new Mensaje
                {
                    ConversacionId = conv!.Id,
                    Contenido = convasacionId ?? "Url no generada",
                    Remitente = TipoRemitente.Sistema,
                    FechaEnvio = DateTime.UtcNow
                };

                _context.Add(msg);
                await _context.SaveChangesAsync();
                return Redirect(url!);
            }
            else
            {
                return BadRequest("Url de destino no valida");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al redirigir");
            return BadRequest(ex.Message);
        }
    }
}