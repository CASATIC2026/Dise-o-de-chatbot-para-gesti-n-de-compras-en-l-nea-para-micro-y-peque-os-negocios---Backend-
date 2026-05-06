using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/inventario")]
public class MensajeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MensajeController> _logger;

    public MensajeController(ApplicationDbContext context, ILogger<MensajeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/inventario/mensajes
    [HttpGet("mensajes")]
    public async Task<ActionResult<object>> GetMensajes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 10, 100);

        var query = _context.Mensajes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            var isNumericSearch = int.TryParse(normalizedSearch, out var numericSearch);

            query = query.Where(m =>
                m.Contenido.ToLower().Contains(normalizedSearch) ||
                (isNumericSearch && (m.ConversacionId == numericSearch || (int)m.Remitente == numericSearch)));
        }

        var totalItems = await query.CountAsync();
        var mensajes = await query
            .OrderByDescending(m => m.FechaEnvio)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            items = mensajes,
            page,
            pageSize,
            totalItems,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        });
    }

    // GET: api/inventario/mensajes/{id}
    [HttpGet("mensajes/{id}")]
    public async Task<ActionResult<Mensaje>> GetMensaje(int id)
    {
        var mensaje = await _context.Mensajes.FindAsync(id);

        if (mensaje == null)
        {
            return NotFound(new { message = "Mensaje no encontrado" });
        }

        return Ok(mensaje);
    }

    // POST: api/inventario/mensajes
    [HttpPost("mensajes")]
    public async Task<ActionResult<Mensaje>> CreateMensaje([FromBody] Mensaje mensaje)
    {
        mensaje.FechaEnvio = DateTime.UtcNow;

        _context.Mensajes.Add(mensaje);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Mensaje creado: {MensajeId}", mensaje.Id);

        return CreatedAtAction(nameof(GetMensaje), new { id = mensaje.Id }, mensaje);
    }

    // PUT: api/inventario/mensajes/{id}
    [HttpPut("mensajes/{id}")]
    public async Task<IActionResult> UpdateMensaje(int id, [FromBody] Mensaje mensaje)
    {
        if (id != mensaje.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var mensajeExistente = await _context.Mensajes.FindAsync(id);
        if (mensajeExistente == null)
        {
            return NotFound(new { message = "Mensaje no encontrado" });
        }

        mensajeExistente.ConversacionId = mensaje.ConversacionId;
        mensajeExistente.Contenido = mensaje.Contenido;
        mensajeExistente.Remitente = mensaje.Remitente;
        // FechaEnvio normalmente no se actualiza, pero si se deseara:
        // mensajeExistente.FechaEnvio = mensaje.FechaEnvio;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Mensaje actualizado: {MensajeId}", id);

        return NoContent();
    }

    // DELETE: api/inventario/mensajes/{id}
    [HttpDelete("mensajes/{id}")]
    public async Task<IActionResult> DeleteMensaje(int id)
    {
        var mensaje = await _context.Mensajes.FindAsync(id);

        if (mensaje == null)
        {
            return NotFound(new { message = "Mensaje no encontrado" });
        }

        _context.Mensajes.Remove(mensaje);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Mensaje eliminado: {MensajeId}", id);

        return NoContent();
    }
}
