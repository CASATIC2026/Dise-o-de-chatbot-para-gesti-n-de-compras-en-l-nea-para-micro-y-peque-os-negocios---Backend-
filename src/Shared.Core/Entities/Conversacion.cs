using System;
using System.Collections.Generic;

namespace Shared.Core.Entities
{
    /// <summary>
    /// Represents a chat conversation between a client and the chatbot or support system.
    /// </summary>
    public class Conversacion
    {
        /// <summary>
        /// Gets or sets the unique identifier for the conversation.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key for the client associated with this conversation.
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the client.
        /// </summary>
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Gets or sets the subject of the conversation or a tracking reference (often a message ID).
        /// </summary>
        public string? Asunto { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the conversation is currently active.
        /// </summary>
        public bool Activa { get; set; } = true;

        /// <summary>
        /// Gets or sets the collection of messages belonging to this conversation.
        /// </summary>
        public ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();

        /// <summary>
        /// Gets or sets the date and time when the conversation was created.
        /// </summary>
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the conversation was last updated.
        /// </summary>
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
    }
}