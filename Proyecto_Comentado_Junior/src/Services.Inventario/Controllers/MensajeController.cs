using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Inventario.Controllers;

/**
 * [EXPLICACIÓN PARA JUNIOR]
 * Este es un "Controlador" de Web API en .NET Core. Su propósito es recibir las solicitudes HTTP
 * (como GET, POST, PUT, DELETE) que vienen desde el frontend (ej. React) o clientes externos.
 * Luego procesa estas solicitudes (interactuando con la base de datos a través de Entity Framework)
 * y retorna una respuesta JSON.
 * 
 * Etiquetas importantes:
 * - [ApiController]: Activa comportamientos útiles para APIs (ej. validación automática de modelos).
 * - [Route("api/admin/mensajes")]: Define la URL base a la cual responderá este controlador.
 */
[ApiController]
[Route("api/admin/mensajes")]
public class MensajeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MensajeController> _logger;

    // [EXPLICACIÓN PARA JUNIOR]
    // Inyección de Dependencias: El constructor pide lo que necesita para funcionar.
    // .NET automáticamente inyectará una instancia de ApplicationDbContext (para la base de datos)
    // y de ILogger (para registrar logs/mensajes en la consola).
    public MensajeController(ApplicationDbContext context, ILogger<MensajeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/admin/mensajes
    // [EXPLICACIÓN PARA JUNIOR]
    // Este método responde a solicitudes GET. 'Task<ActionResult<...>>' significa que la operación
    // es asíncrona (no bloquea el hilo principal) y que devolverá un resultado HTTP (ej. 200 OK)
    // envolviendo una lista de Mensajes.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Mensaje>>> GetMensajes()
    {
        // Consultamos la tabla Mensajes a través de EntityFramework, ordenando de más reciente a más antiguo.
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
