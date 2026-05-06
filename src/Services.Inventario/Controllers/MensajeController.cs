using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Services.Inventario.Validators;

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
    public async Task<ActionResult<IEnumerable<Mensaje>>> GetMensajes()
    {
        var mensajes = await _context.Mensajes
            .OrderByDescending(m => m.FechaEnvio)
            .ToListAsync();

        return Ok(mensajes);
    }

    // GET: api/inventario/mensajes/paged
    [HttpGet("mensajes/paged")]
    public async Task<ActionResult<PagedResult<Mensaje>>> GetMensajesPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
    {
        var query = _context.Mensajes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(m => m.Contenido.ToLower().Contains(s) || m.ConversacionId.ToString().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.FechaEnvio)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Mensaje> { Items = items, TotalCount = total });
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
