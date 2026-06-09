using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Services.Inventario.Validators;

namespace Services.Inventario.Controllers;

/// <summary>
/// API Controller for managing conversations within the inventory system.
/// </summary>
[ApiController]
[Route("api/inventario")]
public class ConversacionController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConversacionController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversacionController"/> class.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    /// <param name="logger">The logger for the controller.</param>
    public ConversacionController(ApplicationDbContext context, ILogger<ConversacionController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all conversations, ordered by their last update date in descending order.
    /// </summary>
    /// <returns>A list of all conversations.</returns>
    // GET: api/inventario/conversaciones
    [HttpGet("conversaciones")]
    public async Task<ActionResult<IEnumerable<Conversacion>>> GetConversaciones()
    {
        var conversaciones = await _context.Conversaciones
            .OrderByDescending(c => c.ActualizadoEn)
            .ToListAsync();

        return Ok(conversaciones);
    }

    /// <summary>
    /// Retrieves a paged result of conversations with optional search filtering.
    /// </summary>
    /// <param name="page">The page number (defaults to 1).</param>
    /// <param name="pageSize">The number of items per page (defaults to 10).</param>
    /// <param name="search">A string to filter conversations by client ID or conversation ID.</param>
    /// <returns>A paged result containing the requested conversations.</returns>
    // GET: api/inventario/conversaciones/paged
    [HttpGet("conversaciones/paged")]
    public async Task<ActionResult<PagedResult<Conversacion>>> GetConversacionesPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
    {
        var query = _context.Conversaciones.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // En conversaciones solemos buscar por ClienteId o ID
            query = query.Where(c => c.ClienteId.ToString().Contains(search) || c.Id.ToString().Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.ActualizadoEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Conversacion> { Items = items, TotalCount = total });
    }

    /// <summary>
    /// Retrieves a specific conversation by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the conversation to retrieve.</param>
    /// <returns>The requested conversation if found; otherwise, a 404 Not Found response.</returns>
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

    /// <summary>
    /// Creates a new conversation and sets the server-side timestamps.
    /// </summary>
    /// <param name="conversacion">The conversation object to be created.</param>
    /// <returns>The newly created conversation with its assigned ID.</returns>
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

    /// <summary>
    /// Updates an existing conversation's details.
    /// </summary>
    /// <param name="id">The ID of the conversation to update.</param>
    /// <param name="conversacion">The conversation data to update.</param>
    /// <returns>A 204 No Content response on success, or an error status code.</returns>
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

    /// <summary>
    /// Permanently deletes a conversation from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the conversation to remove.</param>
    /// <returns>A 204 No Content response on success, or a 404 Not Found response if not found.</returns>
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
