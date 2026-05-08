using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Pagos.Controllers;

/// <summary>
/// API Controller that acts as a proxy to track payment redirections,
/// log transaction references into the chat history, and notify the chatbot service.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="context">The application database context.</param>
/// <param name="httpClientFactory">The factory to create HTTP clients.</param>
/// <param name="configuration">The system configuration.</param>
[ApiController]
[Route("api/pagos")]
public class ProxyController(ILogger<ProxyController> logger, ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration) : ControllerBase
{
    private readonly ILogger<ProxyController> _logger = logger;
    private readonly ApplicationDbContext _context = context;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Tracks a payment redirection request, records the transaction reference as a message in the conversation history,
    /// and redirects the user to the destination gateway URL.
    /// </summary>
    /// <param name="url">The destination payment gateway URL.</param>
    /// <param name="convasacionId">The conversation identifier used to link the redirection to a specific chat (maps to the 'Asunto' field).</param>
    /// <param name="refe">The transaction reference generated for the payment.</param>
    /// <returns>A <see cref="RedirectResult"/> to the target URL or a <see cref="BadRequestObjectResult"/> if parameters are missing.</returns>
    [HttpGet("redirect")]
    public async Task<IActionResult> RastrearRedirigir(
    [FromQuery] string url,
    [FromQuery] string convasacionId,
    [FromQuery] string refe = ""

    )
    {

        Console.WriteLine("Url en metodo de redirect:" + url + "idConv: " + convasacionId);
        try
        {
            var chatBotBaseUrl = _configuration["Services:ChatBotBaseUrl"] ?? "http://chatbot-service:8080";
            using var client = _httpClientFactory.CreateClient();
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(convasacionId) && !string.IsNullOrEmpty(refe))
            {
                //Console.WriteLine("Url en metodo de redirect" + url);
                var conv = await _context.Conversaciones.Include(c => c.Cliente).
                OrderByDescending(c => c.CreadoEn).
                FirstOrDefaultAsync(c => c.Asunto == convasacionId.ToString());

                var pedido = await _context.Pedidos.OrderByDescending(p => p.CreadoEn).
                FirstOrDefaultAsync(p => p.ClienteId == conv!.ClienteId && p.Estado == EstadoPedido.Confirmado);

                var msg = new Mensaje
                {
                    ConversacionId = conv!.Id,
                    Contenido = refe ?? "Url no generada",
                    Remitente = TipoRemitente.Sistema,
                    FechaEnvio = DateTime.UtcNow
                };
                Console.WriteLine("Referencia Wompi: " + refe);
                _context.Add(msg);
                await _context.SaveChangesAsync();
                try
                {
                    await client.PostAsJsonAsync($"{chatBotBaseUrl}/api/bot/pago-procesando", new
                    {
                        referencia = refe,
                        estado = EstadoPedido.Confirmado,
                        url
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo realizar la notificacion al servicio de chat bot.");
                }
                return Redirect(url!);
            }
            else if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(convasacionId) && string.IsNullOrEmpty(refe))
            {
                return Redirect(url!);
            }
            else
            {
                Console.WriteLine("Url en metodo de redirect" + url);
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