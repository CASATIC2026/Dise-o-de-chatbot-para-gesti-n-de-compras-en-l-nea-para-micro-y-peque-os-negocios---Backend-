using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Shared.Core.Entities
{
    /// <summary>
    /// Represents a notification message to be sent within the system, typically for real-time updates.
    /// </summary>
    public class Notificacion
    {
        /// <summary>
        /// Gets or sets the unique identifier for the notification.
        /// </summary>
        public int Id {get; set;}

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        public string Titulo {get; set;}

        /// <summary>
        /// Gets or sets the main message content of the notification.
        /// </summary>
        public string Mensaje {get; set;}

        /// <summary>
        /// Gets or sets the type of the notification (e.g., "Info", "Success", "Warning", "Error").
        /// </summary>
        public string Tipo {get; set;} = "Info"; // info, succes, warning, error

        /// <summary>
        /// Gets or sets the date and time when the notification was created or sent.
        /// </summary>
        public DateTime Fecha {get; set;} = DateTime.UtcNow;

    }
}