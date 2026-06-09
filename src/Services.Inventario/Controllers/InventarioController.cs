using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core.Entities;
using Services.Inventario.Validators;

namespace Services.Inventario.Controllers;

/// <summary>
/// API Controller for managing products and stock inventory.
/// </summary>
[ApiController]
[Route("api/inventario")]
public class InventarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventarioController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventarioController"/> class.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    /// <param name="logger">The logger instance.</param>
    public InventarioController(ApplicationDbContext context, ILogger<InventarioController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of products, optionally filtering for only active ones.
    /// </summary>
    /// <param name="soloActivos">Whether to return only active products (defaults to true).</param>
    /// <returns>A list of products including their category information.</returns>
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
            .OrderBy(p => p.Nombre)
            .Include(p => p.Categoria)
            .ToListAsync();

        return Ok(productos);
    }

    /// <summary>
    /// Retrieves a paged result of products with optional search filtering across name, description, and category.
    /// </summary>
    /// <param name="page">The page number (defaults to 1).</param>
    /// <param name="pageSize">The number of items per page (defaults to 10).</param>
    /// <param name="search">A string to filter products.</param>
    /// <returns>A paged result containing the requested products.</returns>
    // GET: api/inventario/productos/paged
    [HttpGet("productos/paged")]
    public async Task<ActionResult<PagedResult<Producto>>> GetProductosPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
    {
        var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(s) || 
                                    (p.Descripcion != null && p.Descripcion.ToLower().Contains(s)) ||
                                    (p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Producto> { Items = items, TotalCount = total });
    }

    /// <summary>
    /// Retrieves a specific product by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the product to retrieve.</param>
    /// <returns>The requested product if found; otherwise, a 404 Not Found response.</returns>
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

    /// <summary>
    /// Creates a new product and sets the server-side timestamps.
    /// </summary>
    /// <param name="producto">The product object to be created.</param>
    /// <returns>The newly created product with its assigned ID.</returns>
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

    /// <summary>
    /// Updates an existing product's details.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="producto">The product data to update.</param>
    /// <returns>A 204 No Content response on success, or an error status code.</returns>
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
        productoExistente.StockTotal = producto.StockTotal;
        productoExistente.ImagenUrl = producto.ImagenUrl;
        productoExistente.Activo = producto.Activo;
        productoExistente.ActualizadoEn = DateTime.UtcNow;
        productoExistente.CategoriaId = producto.CategoriaId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Producto actualizado: {ProductoId}", id);

        return NoContent();
    }

    /// <summary>
    /// Performs a soft delete on a product by setting its Active status to false.
    /// </summary>
    /// <param name="id">The unique identifier of the product to deactivate.</param>
    /// <returns>A 204 No Content response on success, or a 404 Not Found response if not found.</returns>
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

    /// <summary>
    /// Permanently deletes a product from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the product to remove.</param>
    /// <returns>A 204 No Content response on success, or a 404 Not Found response if not found.</returns>
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
    /// <summary>
    /// Retrieves a paged list of active products with available stock for a specific category.
    /// </summary>
    /// <param name="categoriaId">The ID of the category.</param>
    /// <param name="page">The page number (zero-based).</param>
    /// <param name="pageSize">The number of items per page (defaults to 4).</param>
    /// <returns>A paged result of products within the category.</returns>
    [HttpGet("productos/list-4/{categoriaId}")]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductosList(int categoriaId,
        [FromQuery] int page = 0, [FromQuery] int pageSize = 4
    )
    {

        var Productos = await _context.Productos
            .Where(p => p.CategoriaId == categoriaId)
            .Where(p => p.Activo == true && p.StockDisponible > 0)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await _context.Productos
            .Where(p => p.CategoriaId == categoriaId)
            .Where(p => p.Activo == true && p.StockDisponible > 0)
            .CountAsync();

        return Ok(new PagedResult<Producto> { Items = Productos, TotalCount = total });
    }

    /// <summary>
    /// Temporarily reserves stock for a product.
    /// </summary>
    /// <param name="request">The reservation request containing product ID and quantity.</param>
    /// <returns>A success message and reservation details if stock was available.</returns>
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

        if (producto.StockTotal < request.Cantidad)
        {
            return BadRequest(new
            {
                message = "Stock insuficiente",
                stockDisponible = producto.StockTotal,
                stockSolicitado = request.Cantidad
            });
        }

        // Reserve stock (decrease temporarily)
        producto.StockTotal -= request.Cantidad;
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
            stockRestante = producto.StockTotal
        });
    }

    /// <summary>
    /// Confirms a previously created stock reservation.
    /// </summary>
    /// <param name="request">The confirmation request containing the reservation ID.</param>
    /// <returns>A confirmation message.</returns>
    // POST: api/inventario/confirmar-reserva
    [HttpPost("confirmar-reserva")]
    public async Task<IActionResult> ConfirmarReserva([FromBody] ConfirmarReservaRequest request)
    {
        // In a real implementation, this would validate the reservation ID and mark it as confirmed
        _logger.LogInformation("Reserva confirmada: {ReservaId}", request.ReservaId);

        return Ok(new { message = "Reserva confirmada" });
    }

    /// <summary>
    /// Cancels a stock reservation and returns the stock to the total count.
    /// </summary>
    /// <param name="request">The cancellation request containing the reservation details.</param>
    /// <returns>A message indicating the stock was returned.</returns>
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
        producto.StockTotal += request.Cantidad;
        producto.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Reserva cancelada: {ReservaId}, Stock devuelto: {Cantidad}",
            request.ReservaId, request.Cantidad);

        return Ok(new { message = "Reserva cancelada, stock devuelto" });
    }
}

/// <summary>
/// Request model for confirming a stock reservation.
/// </summary>
public class ConfirmarReservaRequest
{
    /// <summary>
    /// The unique identifier for the reservation.
    /// </summary>
    public string ReservaId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for canceling a stock reservation and returning stock.
/// </summary>
public class CancelarReservaRequest
{
    /// <summary>
    /// The unique identifier for the reservation to cancel.
    /// </summary>
    public string ReservaId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the product whose stock is being returned.
    /// </summary>
    public int ProductoId { get; set; }

    /// <summary>
    /// The quantity of stock to be returned.
    /// </summary>
    public int Cantidad { get; set; }
}
