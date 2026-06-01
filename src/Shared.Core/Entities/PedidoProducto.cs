namespace Shared.Core.Entities;

/// <summary>
/// Represents a specific product item within a customer order, 
/// acting as a join entity between orders and products.
/// </summary>
public class PedidoProducto
{
    /// <summary>
    /// Gets or sets the unique identifier for the order-product entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key of the associated order.
    /// </summary>
    public int PedidoId { get; set; }
    /// <summary>
    /// Gets or sets the navigation property for the associated order.
    /// </summary>
    public Pedido Pedido { get; set; }

    /// <summary>
    /// Gets or sets the foreign key of the associated product.
    /// </summary>
    public int ProductoId { get; set; }
    /// <summary>
    /// Gets or sets the navigation property for the associated product.
    /// </summary>
    public Producto Producto { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the product ordered.
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product at the time the order was placed.
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this item was added to the order.
    /// </summary>
    public DateTime CreadoEn { get; set; }
}