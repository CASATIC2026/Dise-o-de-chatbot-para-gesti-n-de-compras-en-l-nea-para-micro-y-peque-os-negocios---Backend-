using System.ComponentModel.DataAnnotations;

namespace Shared.Core.Entities;

/// <summary>
/// Represents a product within the inventory system, including stock management and category association.
/// </summary>
public class Producto
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the product.
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unit price of the product.
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// Gets or sets the total physical stock available in the warehouse.
    /// </summary>
    public int StockTotal { get; set; }

    /// <summary>
    /// Gets or sets the amount of stock currently reserved for pending orders.
    /// </summary>
    public int? StockReservado {get; set;}

    /// <summary>
    /// Gets the stock available for new sales (usually StockTotal - StockReservado).
    /// </summary>
    public int? StockDisponible {get; private set;}

    /// <summary>
    /// Gets or sets the URL for the product's display image.
    /// </summary>
    public string? ImagenUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is currently active and visible for sale.
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time when the product record was created.
    /// </summary>
    public DateTime CreadoEn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the product record was last updated.
    /// </summary>
    public DateTime ActualizadoEn { get; set; }

    /// <summary>
    /// Gets or sets the collection of order associations where this product is included.
    /// This represents the N:N relationship with orders via the join entity.
    /// </summary>
    public ICollection<PedidoProducto> PedidoProductos { get; set; } = new List<PedidoProducto>();

    /// <summary>
    /// Gets or sets the foreign key of the category this product belongs to.
    /// </summary>
    public int CategoriaId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the associated category.
    /// </summary>
    public Categoria? Categoria { get; set; }
}
