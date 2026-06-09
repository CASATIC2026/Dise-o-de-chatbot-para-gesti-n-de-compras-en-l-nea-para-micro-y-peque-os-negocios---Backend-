using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Producto"/> entity.
    /// Defines table name, primary key, property constraints (including computed columns), and relationships.
    /// </summary>
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        /// <summary>
        /// Configures the entity properties and relationships for <see cref="Producto"/>.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            // 1. Table name
            // Sets the table name for the Producto entity in the database.
            builder.ToTable("Productos");

            // 2. Primary Key
            // Configures the 'Id' property as the unique primary key.
            builder.HasKey(p => p.Id);

            // 3. Properties
            // Configures 'Nombre' and 'Descripcion' with specific maximum lengths.
            builder.Property(p => p.Nombre).HasMaxLength(50);
            builder.Property(p => p.Descripcion).HasMaxLength(200);

            // Ensures financial and inventory properties are required.
            builder.Property(p => p.Precio).IsRequired();
            builder.Property(p => p.StockTotal).IsRequired();

            // Configures stock reservation with a default value.
            builder.Property(p => p.StockReservado).HasDefaultValue(0);

            // Configures 'StockDisponible' as a computed column in PostgreSQL.
            // This column is stored to improve query performance.
            builder.Property(p => p.StockDisponible).HasComputedColumnSql("\"StockTotal\" -\"StockReservado\"", stored: true);

            // Ensures image URL is provided.
            builder.Property(p => p.ImagenUrl).IsRequired();

            // Sets the default active status to true.
            builder.Property(p => p.Activo).HasDefaultValue(true);

            // Configures auditing timestamps to use PostgreSQL's CURRENT_TIMESTAMP by default.
            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 4. Relations
            // Many to One
            // A product can be part of many order line items (PedidoProductos).
            builder.HasMany(p => p.PedidoProductos).
                   WithOne(d => d.Producto).
                   HasForeignKey(p => p.ProductoId);

            // One to Many
            // A product belongs to exactly one category.
            // Cascade delete ensures products are removed if their category is deleted.
            builder.HasOne(p => p.Categoria).
                    WithMany(d => d.Productos).
                    HasForeignKey(p => p.CategoriaId).
                    OnDelete(DeleteBehavior.Cascade);
        }
    }
}