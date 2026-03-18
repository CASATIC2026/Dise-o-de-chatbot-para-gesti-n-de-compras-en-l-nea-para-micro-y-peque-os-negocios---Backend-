using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Services.Inventario.Validators;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/inventario")]
public class InventarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventarioController> _logger;

    public InventarioController(ApplicationDbContext context, ILogger<InventarioController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/inventario/productos
    [HttpGet("productos")]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductos([FromQuery] bool? soloActivos = true)
    {
        var query = _context.Productos.AsQueryable();

        if (soloActivos == true)
        {
            query = query.Where(p => p.Activo);
        }
        // Cargar la informacion del producto junto con la categoria listado ordenado por nombre de producto 
        var productos = await query
            .OrderBy(p => p.Nombre).
            Include(p => p.Categoria)
            .ToListAsync();

        return Ok(productos);
    }

    // GET: api/inventario/productos/{id}
    [HttpGet("productos/{id}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }

        return Ok(producto);
    }

    // POST: api/inventario/productos
    [HttpPost("productos")]
    public async Task<ActionResult<Producto>> CreateProducto([FromBody] Producto producto)
    {
        producto.CreadoEn = DateTime.UtcNow;
        producto.ActualizadoEn = DateTime.UtcNow;

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Producto creado: {ProductoId} - {Nombre}", producto.Id, producto.Nombre);

        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    // PUT: api/inventario/productos/{id}
    [HttpPut("productos/{id}")]
    public async Task<IActionResult> UpdateProducto(int id, [FromBody] Producto producto)
    {
        if (id != producto.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var productoExistente = await _context.Productos.FindAsync(id);
        if (productoExistente == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }
        // Actualizar solo los campos permitidos
        productoExistente.Nombre = producto.Nombre;
        productoExistente.Descripcion = producto.Descripcion;
        productoExistente.Precio = producto.Precio;
        productoExistente.Stock = producto.Stock;
        productoExistente.ImagenUrl = producto.ImagenUrl;
        productoExistente.Activo = producto.Activo;
        productoExistente.ActualizadoEn = DateTime.UtcNow;
        productoExistente.CategoriaId = producto.CategoriaId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Producto actualizado: {ProductoId}", id);

        return NoContent();
    }

    // DELETE: api/inventario/productos/{id}
    [HttpDelete("productos/soft-delete/{id}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }

        // Soft delete
        producto.Activo = false;
        producto.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Producto desactivado: {ProductoId}", id);

        return NoContent();
    }
    // DELETE: api/inventario/productos/{id}
    [HttpDelete("productos/{id}")]
    public async Task<IActionResult> DeleteProductoPermanente(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }
        // Delete permanently
        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Producto eliminado: {ProductoId}", id);

        return NoContent();
    }

    //Personal Queries 
    [HttpGet("productos/list-4/{categoriaId}")]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductosList(int categoriaId,
        [FromQuery] int page = 0, [FromQuery] int pageSize = 4
    )
    {
        var total = await _context.Productos.CountAsync();
        var Productos = await _context.Productos.
        Where(p => p.CategoriaId == categoriaId).
        Where(p => p.Activo == true).
        Skip(page * pageSize).
        Take(pageSize).
        ToListAsync();

        return Ok(new PagedResult<Producto> { Items = Productos, TotalCount = total });
    }
    // POST: api/inventario/reservar
    [HttpPost("reservar")]
    public async Task<IActionResult> ReservarStock([FromBody] ReservaProductoRequest request)
    {
        var validator = new ReservaProductoValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var producto = await _context.Productos.FindAsync(request.ProductoId);

        if (producto == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }

        if (!producto.Activo)
        {
            return BadRequest(new { message = "Producto no disponible" });
        }

        if (producto.Stock < request.Cantidad)
        {
            return BadRequest(new
            {
                message = "Stock insuficiente",
                stockDisponible = producto.Stock,
                stockSolicitado = request.Cantidad
            });
        }

        // Reserve stock (decrease temporarily)
        producto.Stock -= request.Cantidad;
        producto.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var reservaId = request.ReservaId ?? Guid.NewGuid().ToString();

        _logger.LogInformation("Stock reservado: Producto {ProductoId}, Cantidad {Cantidad}, ReservaId {ReservaId}",
            request.ProductoId, request.Cantidad, reservaId);

        return Ok(new
        {
            message = "Stock reservado exitosamente",
            reservaId = reservaId,
            productoId = producto.Id,
            cantidadReservada = request.Cantidad,
            stockRestante = producto.Stock
        });
    }

    // POST: api/inventario/confirmar-reserva
    [HttpPost("confirmar-reserva")]
    public async Task<IActionResult> ConfirmarReserva([FromBody] ConfirmarReservaRequest request)
    {
        // In a real implementation, this would validate the reservation ID and mark it as confirmed
        _logger.LogInformation("Reserva confirmada: {ReservaId}", request.ReservaId);

        return Ok(new { message = "Reserva confirmada" });
    }

    // POST: api/inventario/cancelar-reserva
    [HttpPost("cancelar-reserva")]
    public async Task<IActionResult> CancelarReserva([FromBody] CancelarReservaRequest request)
    {
        var producto = await _context.Productos.FindAsync(request.ProductoId);

        if (producto == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }

        // Return stock
        producto.Stock += request.Cantidad;
        producto.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Reserva cancelada: {ReservaId}, Stock devuelto: {Cantidad}",
            request.ReservaId, request.Cantidad);

        return Ok(new { message = "Reserva cancelada, stock devuelto" });
    }
}

public class ConfirmarReservaRequest
{
    public string ReservaId { get; set; } = string.Empty;
}

public class CancelarReservaRequest
{
    public string ReservaId { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}
