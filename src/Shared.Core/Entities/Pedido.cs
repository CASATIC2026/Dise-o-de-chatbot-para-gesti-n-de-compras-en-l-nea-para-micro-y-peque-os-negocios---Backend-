namespace Shared.Core.Entities;

public enum EstadoPedido
{
    Pendiente,
    Confirmado,
    Pagado,
    Enviado,
    Cancelado
}

public class Pedido
{
    public int Id { get; set; }

    // FK Usuario
    public int UsuarioId { get; set; }

    // FK Cliente
    public int ClienteId { get; set; }

    // Estado del pedido
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

    // Total del pedido
    public decimal Total { get; set; }

    // Dirección de entrega
    public string DireccionEntrega { get; set; } = string.Empty;

    // JSONB para los productos del pedido
    public string DetallesJson { get; set; } = "[]";

    // Referencia de pago Wompi
    public string? ReferenciaWompi { get; set; }

    // Auditoría
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Usuario Usuario { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public Pago? Pago { get; set; }

    public ICollection<PedidoProducto> PedidoProductos { get; set; } = new List<PedidoProducto>();
}