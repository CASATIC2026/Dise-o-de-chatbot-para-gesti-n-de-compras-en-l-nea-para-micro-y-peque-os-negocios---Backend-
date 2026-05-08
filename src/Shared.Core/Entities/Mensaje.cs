using System;

namespace Shared.Core.Entities
{
    /// <summary>
    /// Defines the types of senders that can participate in a conversation.
    /// </summary>
    public enum TipoRemitente
    {
        /// <summary>The end client or customer.</summary>
        Cliente = 1,
        /// <summary>A support agent or human operator.</summary>
        Soporte = 2,
        /// <summary>The automated system or chatbot.</summary>
        Sistema = 3
    }

    /// <summary>
    /// Represents an individual message within a conversation.
    /// </summary>
    public class Mensaje
    {
        /// <summary>
        /// Gets or sets the unique identifier for the message.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the conversation this message belongs to.
        /// </summary>
        public int ConversacionId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the associated conversation.
        /// </summary>
        public Conversacion? Conversacion { get; set; }

        /// <summary>
        /// Gets or sets the text content of the message.
        /// </summary>
        public string Contenido { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of sender who created this message.
        /// </summary>
        public TipoRemitente Remitente { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message was sent.
        /// </summary>
        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    }
}