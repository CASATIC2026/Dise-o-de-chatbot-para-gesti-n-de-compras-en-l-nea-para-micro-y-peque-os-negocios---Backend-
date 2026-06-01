namespace Shared.Core.Entities
{
    /// <summary>
    /// Represents a client within the system, storing personal details, 
    /// social media integration IDs, and transaction history.
    /// </summary>
    public class Cliente
    {
        /// <summary>
        /// Gets or sets the unique identifier for the client.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the full name of the client.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contact phone number of the client.
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Gets or sets the email address of the client.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the physical delivery address of the client.
        /// </summary>
        public string? Direccion { get; set; }

        /// <summary>
        /// Gets or sets the Telegram ID used for chatbot identification and communication.
        /// </summary>
        public long? TelegramId { get; set; }

        /// <summary>
        /// Gets or sets the WhatsApp ID/Number used for integration purposes.
        /// </summary>
        public string? WhatsAppId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the client record was created.
        /// </summary>
        public DateTime CreadoEn { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the client record was last updated.
        /// </summary>
        public DateTime ActualizadoEn { get; set; }

        /// <summary>
        /// Gets or sets the conversation history stored as a JSON string for auditing or bot context.
        /// </summary>
        public string? HistorialConversacion { get; set; } = "[]";

        /// <summary>
        /// Gets or sets the collection of orders associated with this client.
        /// </summary>
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

        /// <summary>
        /// Gets or sets the collection of conversations linked to this client.
        /// </summary>
        public ICollection<Conversacion> Conversaciones { get; set; } = new List<Conversacion>();
    }
}