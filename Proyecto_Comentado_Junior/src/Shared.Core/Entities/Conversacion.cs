using System;
using System.Collections.Generic;

namespace Shared.Core.Entities
{
    public class Conversacion
    {
        public int Id { get; set; }

        // Relación con Cliente
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public string? Asunto { get; set; }

        public bool Activa { get; set; } = true;

        // Una conversación puede tener muchos mensajes
        public ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();

        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
    }
}