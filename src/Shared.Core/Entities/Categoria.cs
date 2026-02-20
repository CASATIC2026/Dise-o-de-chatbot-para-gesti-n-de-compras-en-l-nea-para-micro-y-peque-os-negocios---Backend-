using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Core.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        // Relación 1:N → Una categoría tiene muchos productos
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();

        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
    }
}