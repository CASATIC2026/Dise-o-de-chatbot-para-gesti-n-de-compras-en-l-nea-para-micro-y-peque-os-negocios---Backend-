namespace Shared.Core.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }

        // Propiedades para integraciones y auditoría
        public long? TelegramId { get; set; }
        public string? WhatsAppId { get; set; }

        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        // JSONB column for conversation history
        public string? HistorialConversacion { get; set; } = "[]";
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public ICollection<Conversacion> Conversaciones { get; set; } = new List<Conversacion>();
    }
}