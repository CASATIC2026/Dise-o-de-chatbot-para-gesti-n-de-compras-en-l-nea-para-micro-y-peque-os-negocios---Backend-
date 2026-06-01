using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    /// <summary>
    /// Configures the database mapping for the <see cref="PedidoProducto"/> entity.
    /// Defines table name, primary key, property constraints, and relationships.
    /// </summary>
    public class PedidoProductoConfiguration : IEntityTypeConfiguration<PedidoProducto>
    {
        /// <summary>
        /// Configures the entity properties and relationships for <see cref="PedidoProducto"/>.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<PedidoProducto> builder)
        {
            // 1. Table name
            // Sets the table name for the PedidoProducto entity in the database.
            builder.ToTable("PedidoProductos");

            // 2. Primary Key
            // Configures the 'Id' property as the unique primary key.
            builder.HasKey(p => p.Id);

            // 3. Properties
            // Configures 'Cantidad' and 'PrecioUnitario' as required fields.
            builder.Property(p => p.Cantidad).IsRequired();
            builder.Property(p => p.PrecioUnitario).IsRequired();

            // Configures 'CreadoEn' to use PostgreSQL's CURRENT_TIMESTAMP by default.
            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 4. Relations 
            // Many to One
            // Configures the relationship with Producto. 
            // Cascade delete ensures order items are removed if the product is deleted.
            builder.HasOne(p => p.Producto).
                WithMany(d => d.PedidoProductos).
                HasForeignKey(p => p.ProductoId).OnDelete(DeleteBehavior.Cascade);

            // Configures the relationship with Pedido.
            // Cascade delete ensures order items are removed if the associated order is deleted.
            builder.HasOne(p => p.Pedido).
                WithMany(d => d.PedidoProductos).
                HasForeignKey(p => p.PedidoId).OnDelete(DeleteBehavior.Cascade);


        }
    }
}