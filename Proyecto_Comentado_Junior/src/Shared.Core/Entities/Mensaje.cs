using System;

namespace Shared.Core.Entities
{
    public enum TipoRemitente
    {
        Cliente = 1,
        Soporte = 2,
        Sistema = 3
    }

    public class Mensaje
    {
        public int Id { get; set; }

        // Relación con Conversacion
        public int ConversacionId { get; set; }
        public Conversacion Conversacion { get; set; }

        public string Contenido { get; set; } = string.Empty;

        public TipoRemitente Remitente { get; set; }

        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    }
}