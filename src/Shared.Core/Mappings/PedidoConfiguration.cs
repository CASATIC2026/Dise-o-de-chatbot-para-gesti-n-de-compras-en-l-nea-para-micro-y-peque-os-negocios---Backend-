using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Pedido"/> entity.
    /// Defines table name, primary key, property constraints, and relationships with clients, users, payments, and products.
    /// </summary>
    public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
    {
        /// <summary>
        /// Configures the entity properties and relationships for <see cref="Pedido"/>.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            // 1. Table name
            // Sets the table name for the Pedido entity in the database.
            builder.ToTable("Pedidos");

            // 2. Primary Key
            // Configures the 'Id' property as the unique primary key.
            builder.HasKey(p => p.Id);

            // 3. Properties
            // Configures order status with a default value of 'Pendiente'.
            builder.Property(p => p.Estado).HasDefaultValue(EstadoPedido.Pendiente);

            // Ensures total is a required field.
            builder.Property(p => p.Total).IsRequired();

            // Configures delivery address with a max length and default empty string.
            builder.Property(p => p.DireccionEntrega).HasMaxLength(200).HasDefaultValue("");

            // Ensures the audit details JSON is required.
            builder.Property(p => p.DetallesJson).IsRequired();

            // Wompi payment gateway reference.
            builder.Property(p => p.ReferenciaWompi);

            // Configures auditing timestamps to use PostgreSQL's CURRENT_TIMESTAMP by default.
            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 4. Relations 
            // One to One
            // A pedido has one associated payment record.
            // Cascade delete ensures payment is removed if the order is deleted.
            builder.HasOne(p => p.Pago).
                WithOne(d => d.Pedido).
                HasForeignKey<Pago>(d => d.PedidoId).
                OnDelete(DeleteBehavior.Cascade);

            // Many to One
            // Many pedidos belong to one client.
            // Cascade delete ensures orders are cleaned up if a client is removed.
            builder.HasOne(p => p.Cliente).
                WithMany(d => d.Pedidos).
                HasForeignKey(p => p.ClienteId).
                OnDelete(DeleteBehavior.Cascade);

            // Many to One
            // Many pedidos can be managed by one user (staff).
            builder.HasOne(p => p.Usuario).
                WithMany(d => d.Pedidos).
                HasForeignKey(p => p.UsuarioId).
                OnDelete(DeleteBehavior.Cascade);

            // One to Many
            // One pedido has many order line items (PedidoProductos).
            builder.HasMany(p => p.PedidoProductos).
                WithOne(p => p.Pedido).
                HasForeignKey(d => d.PedidoId);
        }
    }
}
