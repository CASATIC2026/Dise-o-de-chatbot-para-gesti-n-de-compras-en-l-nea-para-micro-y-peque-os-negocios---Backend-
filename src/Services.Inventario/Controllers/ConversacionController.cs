using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/inventario")]
public class ConversacionController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConversacionController> _logger;

    public ConversacionController(ApplicationDbContext context, ILogger<ConversacionController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/inventario/conversaciones
    [HttpGet("conversaciones")]
    public async Task<ActionResult<IEnumerable<Conversacion>>> GetConversaciones()
    {
        var conversaciones = await _context.Conversaciones
            .OrderByDescending(c => c.ActualizadoEn)
            .ToListAsync();

        return Ok(conversaciones);
    }

    // GET: api/inventario/conversaciones/{id}
    [HttpGet("conversaciones/{id}")]
    public async Task<ActionResult<Conversacion>> GetConversacion(int id)
    {
        var conversacion = await _context.Conversaciones.FindAsync(id);

        if (conversacion == null)
        {
            return NotFound(new { message = "Conversacion no encontrada" });
        }

        return Ok(conversacion);
    }

    // POST: api/inventario/conversaciones
    [HttpPost("conversaciones")]
    public async Task<ActionResult<Conversacion>> CreateConversacion([FromBody] Conversacion conversacion)
    {
        conversacion.CreadoEn = DateTime.UtcNow;
        conversacion.ActualizadoEn = DateTime.UtcNow;
        //conversacion.Activa = conversacion.Activa;


        _context.Conversaciones.Add(conversacion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Conversacion creada: {ConversacionId}", conversacion.Id);

        return CreatedAtAction(nameof(GetConversacion), new { id = conversacion.Id }, conversacion);
    }

    // PUT: api/inventario/conversaciones/{id}
    [HttpPut("conversaciones/{id}")]
    public async Task<IActionResult> UpdateConversacion(int id, [FromBody] Conversacion conversacion)
    {
        if (id != conversacion.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var conversacionExistente = await _context.Conversaciones.FindAsync(id);
        if (conversacionExistente == null)
        {
            return NotFound(new { message = "Conversacion no encontrada" });
        }

        conversacionExistente.ClienteId = conversacion.ClienteId;
        conversacionExistente.Activa = conversacion.Activa;
        conversacionExistente.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Conversacion actualizada: {ConversacionId}", id);

        return NoContent();
    }

    // DELETE: api/inventario/conversaciones/{id}
    [HttpDelete("conversaciones/{id}")]
    public async Task<IActionResult> DeleteConversacion(int id)
    {
        var conversacion = await _context.Conversaciones.FindAsync(id);

        if (conversacion == null)
        {
            return NotFound(new { message = "Conversacion no encontrada" });
        }

        _context.Conversaciones.Remove(conversacion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Conversacion eliminada: {ConversacionId}", id);

        return NoContent();
    }
}
