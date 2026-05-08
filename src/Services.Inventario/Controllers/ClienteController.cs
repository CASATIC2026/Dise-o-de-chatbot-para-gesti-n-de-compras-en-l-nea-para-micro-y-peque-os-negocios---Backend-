using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Services.Inventario.Validators;

namespace Services.Inventario.Controllers;

/// <summary>
/// API Controller for managing client information.
/// </summary>
[ApiController]
[Route("api/inventario")]
public class ClienteController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClienteController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClienteController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ClienteController(ApplicationDbContext context, ILogger<ClienteController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all clients ordered by name.
    /// </summary>
    /// <returns>A list of clients.</returns>
    // GET: api/inventario/clientes
    [HttpGet("clientes")]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
    {
        var clientes = await _context.Clientes
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return Ok(clientes);
    }

    /// <summary>
    /// Retrieves a paged result of clients with optional search filtering.
    /// </summary>
    /// <param name="page">The page number (defaults to 1).</param>
    /// <param name="pageSize">The number of items per page (defaults to 10).</param>
    /// <param name="search">A string to filter clients by name, email, or phone.</param>
    /// <returns>A paged result containing the requested clients.</returns>
    // GET: api/inventario/clientes/paged
    [HttpGet("clientes/paged")]
    public async Task<ActionResult<PagedResult<Cliente>>> GetClientesPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c => c.Nombre.ToLower().Contains(s) || 
                                    (c.Email != null && c.Email.ToLower().Contains(s)) || 
                                    (c.Telefono != null && c.Telefono.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Cliente> { Items = items, TotalCount = total });
    }

    /// <summary>
    /// Retrieves a specific client by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the client.</param>
    /// <returns>The requested client if found; otherwise, 404 Not Found.</returns>
    // GET: api/inventario/clientes/{id}
    [HttpGet("clientes/{id}")]
    public async Task<ActionResult<Cliente>> GetCliente(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
        {
            return NotFound(new { message = "Cliente no encontrado" });
        }

        return Ok(cliente);
    }

    /// <summary>
    /// Creates a new client.
    /// </summary>
    /// <param name="cliente">The client data to create.</param>
    /// <returns>The created client.</returns>
    // POST: api/inventario/clientes
    [HttpPost("clientes")]
    public async Task<ActionResult<Cliente>> CreateCliente([FromBody] Cliente cliente)
    {
        cliente.CreadoEn = DateTime.UtcNow;
        cliente.ActualizadoEn = DateTime.UtcNow;

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente creado: {ClienteId} - {Nombre}", cliente.Id, cliente.Nombre);

        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
    }

    /// <summary>
    /// Updates an existing client.
    /// </summary>
    /// <param name="id">The unique identifier of the client to update.</param>
    /// <param name="cliente">The updated client data.</param>
    /// <returns>204 No Content if successful; otherwise, an error response.</returns>
    // PUT: api/inventario/clientes/{id}
    [HttpPut("clientes/{id}")]
    public async Task<IActionResult> UpdateCliente(int id, [FromBody] Cliente cliente)
    {
        if (id != cliente.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var clienteExistente = await _context.Clientes.FindAsync(id);
        if (clienteExistente == null)
        {
            return NotFound(new { message = "Cliente no encontrado" });
        }

        clienteExistente.Nombre = cliente.Nombre;
        clienteExistente.Telefono = cliente.Telefono;
        clienteExistente.Email = cliente.Email;
        clienteExistente.Direccion = cliente.Direccion;
        clienteExistente.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente actualizado: {ClienteId}", id);

        return NoContent();
    }

    /// <summary>
    /// Permanently deletes a client from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the client to delete.</param>
    /// <returns>204 No Content if successful; otherwise, 404 Not Found.</returns>
    // DELETE: api/inventario/clientes/{id}
    [HttpDelete("clientes/{id}")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
        {
            return NotFound(new { message = "Cliente no encontrado" });
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente eliminado: {ClienteId}", id);

        return NoContent();
    }
}
