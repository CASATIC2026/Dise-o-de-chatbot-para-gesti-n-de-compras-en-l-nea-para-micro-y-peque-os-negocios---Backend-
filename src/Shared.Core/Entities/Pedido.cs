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
    public int UsuarioId { get; set; }
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
    public decimal Total { get; set; }
    public string DireccionEntrega { get; set; } = string.Empty;
    
    // JSONB column for order line items
    public string DetallesJson { get; set; } = "[]";
    
    public string? ReferenciaWompi { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
    
    // Navigation property
    public Usuario Usuario { get; set; } = null!;
}
