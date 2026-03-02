using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/admin/mensajes")]
public class MensajeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MensajeController> _logger;

    public MensajeController(ApplicationDbContext context, ILogger<MensajeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/admin/mensajes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Mensaje>>> GetMensajes()
    {
        var mensajes = await _context.Mensajes
            .OrderByDescending(m => m.FechaEnvio)
            .ToListAsync();

        return Ok(mensajes);
    }

    // GET: api/admin/mensajes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Mensaje>> GetMensaje(int id)
    {
        var mensaje = await _context.Mensajes.FindAsync(id);

        if (mensaje == null)
        {
            return NotFound(new { message = "Mensaje no encontrado" });
        }

        return Ok(mensaje);
    }

    // POST: api/admin/mensajes
    [HttpPost]
    public async Task<ActionResult<Mensaje>> CreateMensaje([FromBody] Mensaje mensaje)
    {
        mensaje.FechaEnvio = DateTime.UtcNow;

        _context.Mensajes.Add(mensaje);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Mensaje creado: {MensajeId}", mensaje.Id);

        return CreatedAtAction(nameof(GetMensaje), new { id = mensaje.Id }, mensaje);
    }

    // PUT: api/admin/mensajes/{id}
    [HttpPut("{id}")]
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
        mensajeExistente.Role = mensaje.Role;
        // FechaEnvio normalmente no se actualiza, pero si se deseara:
        // mensajeExistente.FechaEnvio = mensaje.FechaEnvio;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Mensaje actualizado: {MensajeId}", id);

        return NoContent();
    }

    // DELETE: api/admin/mensajes/{id}
    [HttpDelete("{id}")]
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
