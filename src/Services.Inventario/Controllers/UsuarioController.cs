using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using BCrypt.Net;
using Services.Inventario.Validators;

namespace Services.Inventario.Controllers;

/// <summary>
/// API Controller for managing system users and their credentials.
/// </summary>
[ApiController]
[Route("api/inventario")]
public class UsuarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsuarioController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsuarioController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public UsuarioController(ApplicationDbContext context, ILogger<UsuarioController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all users ordered by name.
    /// </summary>
    /// <returns>A list of all users.</returns>
    // GET: api/inventario/usuarios
    [HttpGet("usuarios")]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
    {
        var usuarios = await _context.Usuarios
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        return Ok(usuarios);
    }

    /// <summary>
    /// Retrieves a paged result of users with optional search filtering by name, email, or phone.
    /// </summary>
    /// <param name="page">The page number (defaults to 1).</param>
    /// <param name="pageSize">The number of items per page (defaults to 10).</param>
    /// <param name="search">A string to filter users.</param>
    /// <returns>A paged result containing the requested users.</returns>
    // GET: api/inventario/usuarios/paged
    [HttpGet("usuarios/paged")]
    public async Task<ActionResult<PagedResult<Usuario>>> GetUsuariosPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
    {
        var query = _context.Usuarios.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(u => u.Nombre.ToLower().Contains(s) || 
                                    (u.Email != null && u.Email.ToLower().Contains(s)) ||
                                    (u.Telefono != null && u.Telefono.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Usuario> { Items = items, TotalCount = total });
    }

    /// <summary>
    /// Retrieves a specific user by their unique identifier.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The requested user if found; otherwise, 404 Not Found.</returns>
    // GET: api/inventario/usuarios/{id}
    [HttpGet("usuarios/{id}")]
    public async Task<ActionResult<Usuario>> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        return Ok(usuario);
    }

    /// <summary>
    /// Creates a new user and hashes the provided password.
    /// </summary>
    /// <param name="usuario">The user data to create.</param>
    /// <returns>The created user record.</returns>
    // POST: api/inventario/usuarios
    [HttpPost("usuarios")]
    public async Task<ActionResult<Usuario>> CreateUsuario([FromBody] Usuario usuario)
    {
        if (!string.IsNullOrEmpty(usuario.ContrasenaHash))
        {
            usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(usuario.ContrasenaHash);
        }


        usuario.CreadoEn = DateTime.UtcNow;
        usuario.ActualizadoEn = DateTime.UtcNow;

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario creado: {UsuarioId} - {Nombre}", usuario.Id, usuario.Nombre);

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
    }

    /// <summary>
    /// Updates an existing user's details and re-hashes the password if provided.
    /// </summary>
    /// <param name="id">The user ID to update.</param>
    /// <param name="usuario">The updated user data.</param>
    /// <returns>204 No Content if successful; otherwise, an error response.</returns>
    // PUT: api/inventario/usuarios/{id}
    [HttpPut("usuarios/{id}")]
    public async Task<IActionResult> UpdateUsuario(int id, [FromBody] Usuario usuario)
    {
        if (id != usuario.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var usuarioExistente = await _context.Usuarios.FindAsync(id);
        if (usuarioExistente == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        usuarioExistente.Nombre = usuario.Nombre;
        usuarioExistente.Email = usuario.Email;
        // Ideally handled securely via another method if it changes
        if (!string.IsNullOrEmpty(usuario.ContrasenaHash))
        {
            usuarioExistente.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(usuario.ContrasenaHash);
        }
        usuarioExistente.Rol = usuario.Rol;
        usuarioExistente.Estado = usuario.Estado;
        usuarioExistente.Telefono = usuario.Telefono;
        usuarioExistente.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario actualizado: {UsuarioId}", id);

        return NoContent();
    }

    /// <summary>
    /// Permanently deletes a user from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>204 No Content if successful; otherwise, 404 Not Found.</returns>
    // DELETE: api/inventario/usuarios/{id}
    [HttpDelete("usuarios/{id}")]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario eliminado: {UsuarioId}", id);

        return NoContent();
    }
}
