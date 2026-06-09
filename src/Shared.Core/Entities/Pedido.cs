namespace Shared.Core.Entities;

/// <summary>
/// Defines the various states an order can transition through in the system.
/// </summary>
public enum EstadoPedido
{
    /// <summary>The order has been created but not yet confirmed or processed.</summary>
    Pendiente,
    /// <summary>The order has been reviewed and confirmed by the system or staff.</summary>
    Confirmado,
    /// <summary>Payment for the order has been successfully and waiting for the payment.</summary>
    Pagado,
    /// <summary>The items in the order have been dispatched to the delivery address.</summary>
    Enviado,
    /// <summary>The order has been cancelled and will not be fulfilled.</summary>
    Cancelado
}

/// <summary>
/// Represents a customer order, containing transaction details, status, and associations with clients and users.
/// </summary>
public class Pedido
{
    /// <summary>
    /// Gets or sets the unique identifier for the order.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the optional foreign key for the user (staff/admin) who managed the order.
    /// </summary>
    public int? UsuarioId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key for the client who placed the order.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Gets or sets the current status of the order.
    /// </summary>
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

    /// <summary>
    /// Gets or sets the total monetary amount for the order.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Gets or sets the physical address where the order should be delivered.
    /// </summary>
    public string DireccionEntrega { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a JSON string containing additional details about the items in the order for flexible auditing.
    /// </summary>
    public string DetallesJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets the transaction reference provided by the Wompi payment gateway.
    /// </summary>
    public string? ReferenciaWompi { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the order was created.
    /// </summary>
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the order record was last updated.
    /// </summary>
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the navigation property for the user associated with this order.
    /// </summary>
    public Usuario? Usuario { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation property for the client associated with this order.
    /// </summary>
    public Cliente? Cliente { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation property for the payment record linked to this order.
    /// </summary>
    public Pago? Pago { get; set; }

    /// <summary>
    /// Gets or sets the collection of individual product entries associated with this order.
    /// </summary>
    public ICollection<PedidoProducto> PedidoProductos { get; set; } = new List<PedidoProducto>();
}