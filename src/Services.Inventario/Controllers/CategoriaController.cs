using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Services.Inventario.Validators;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/inventario")]
public class CategoriaController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoriaController> _logger;

    public CategoriaController(ApplicationDbContext context, ILogger<CategoriaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/inventario/categorias
    [HttpGet("categorias")]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoria()
    {
        var query = _context.Categorias.AsQueryable();

        var categorias = await query
            .OrderBy(p => p.Nombre).
            Include(p => p.Productos)
            .ToListAsync();

        return Ok(categorias);
    }

    // GET: api/inventario/categorias/{id}
    [HttpGet("categorias/{id}")]
    public async Task<ActionResult<Categoria>> GetCategorias(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);

        if (categoria == null)
        {
            return NotFound(new { message = "Categoria no encontrado" });
        }

        return Ok(categoria);
    }

    // POST: api/inventario/categorias
    [HttpPost("categorias")]
    public async Task<ActionResult<Categoria>> CreateCategoria([FromBody] Categoria categoria)
    {
        categoria.CreadoEn = DateTime.UtcNow;
        categoria.ActualizadoEn = DateTime.UtcNow;

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Categoria creada: {CategoriaId} - {Nombre}", categoria.Id, categoria.Nombre);

        return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
    }

    // PUT: api/inventario/categorias/{id}
    [HttpPut("categorias/{id}")]
    public async Task<IActionResult> UpdateCategoria(int id, [FromBody] Categoria categoria)
    {
        if (id != categoria.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var categoriaExistente = await _context.Categorias.FindAsync(id);
        if (categoriaExistente == null)
        {
            return NotFound(new { message = "Categoria no encontrada" });
        }

        categoriaExistente.Nombre = categoria.Nombre;
        categoriaExistente.Descripcion = categoria.Descripcion;
        categoriaExistente.CreadoEn = DateTime.UtcNow;
        categoriaExistente.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Categoria actualizada: {CategoriaId}", id);

        return NoContent();
    }

    // DELETE: api/inventario/categorias/{id}
    [HttpDelete("categorias/{id}")]
    public async Task<IActionResult> DeleteCategoriaPermanente(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);

        if (categoria == null)
        {
            return NotFound(new { message = "Categoria no encontrada" });
        }
        // Delete permanently
        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Categoria eliminada: {CategoriaId}", id);

        return NoContent();
    }
    //Personal Queries 
    [HttpGet("categorias/list-6")]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoriasList(
        [FromQuery] int page = 0, [FromQuery] int pageSize = 6
    )
    {
        var total = await _context.Categorias.CountAsync();
        var Categorias = await _context.Categorias.
        OrderBy(c => c.Nombre).
        Skip(page * pageSize).
        Take(pageSize).
        ToListAsync();

        return Ok(new PagedResult<Categoria> { Items = Categorias, TotalCount = total });
    }

}