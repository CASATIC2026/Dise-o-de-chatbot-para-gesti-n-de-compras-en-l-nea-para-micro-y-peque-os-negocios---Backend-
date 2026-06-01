using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Core.Entities
{
    /// <summary>
    /// Represents a product category within the inventory system.
    /// </summary>
    public class Categoria
    {
        /// <summary>
        /// Gets or sets the unique identifier for the category.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional description of the category.
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Gets or sets the collection of products associated with this category.
        /// This defines a one-to-many relationship.
        /// </summary>
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();

        /// <summary>
        /// Gets or sets the date and time when the category record was created.
        /// </summary>
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the category record was last updated.
        /// </summary>
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
    }
}