namespace Shared.Core.Entities;

public class Usuario
{
    public int Id { get; set; }
    public long? TelegramId { get; set; }
    public string? WhatsAppId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    
    // JSONB column for conversation history
    public string HistorialConversacion { get; set; } = "[]";
    
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
    
    // Navigation property
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
