using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/admin/usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(ApplicationDbContext context, ILogger<UsuarioController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/admin/usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
    {
        var usuarios = await _context.Usuarios
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        return Ok(usuarios);
    }

    // GET: api/admin/usuarios/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Usuario>> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        return Ok(usuario);
    }

    // POST: api/admin/usuarios
    [HttpPost]
    public async Task<ActionResult<Usuario>> CreateUsuario([FromBody] Usuario usuario)
    {
        usuario.CreadoEn = DateTime.UtcNow;
        usuario.ActualizadoEn = DateTime.UtcNow;

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario creado: {UsuarioId} - {Nombre}", usuario.Id, usuario.Nombre);

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
    }

    // PUT: api/admin/usuarios/{id}
    [HttpPut("{id}")]
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
             usuarioExistente.ContrasenaHash = usuario.ContrasenaHash; 
        }
        usuarioExistente.Rol = usuario.Rol;
        usuarioExistente.Estado = usuario.Estado;
        usuarioExistente.Telefono = usuario.Telefono;
        usuarioExistente.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario actualizado: {UsuarioId}", id);

        return NoContent();
    }

    // DELETE: api/admin/usuarios/{id}
    [HttpDelete("{id}")]
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
