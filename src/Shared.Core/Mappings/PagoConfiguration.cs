using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Pago"/> entity.
    /// Defines the table structure, primary key, property constraints, and the relationship with orders.
    /// </summary>
    public class PagoConfiguration : IEntityTypeConfiguration<Pago>
    {
        /// <summary>
        /// Configures the entity properties and relationships for <see cref="Pago"/>.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            // 1. Table name
            // Sets the table name for the Pago entity in the database.
            builder.ToTable("Pagos");

            // 2. Primary Key
            // Configures the 'Id' property as the unique primary key.
            builder.HasKey(p => p.Id);

            // 3. Properties
            // Configures required fields and constraints.
            builder.Property(p => p.Monto).IsRequired();
            builder.Property(p => p.MetodoPago).IsRequired();
            builder.Property(p => p.Estado).IsRequired();
            builder.Property(p => p.ReferenciaTransaccion).IsRequired();

            // Configures auditing and transaction timestamps to use PostgreSQL's CURRENT_TIMESTAMP by default.
            builder.Property(p => p.FechaPago).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 4. Relations 
            // One to One relationship: A payment belongs to one specific order.
            // Cascade delete ensures the payment record is removed if the associated order is deleted.
            builder.HasOne(p => p.Pedido).
                WithOne(d => d.Pago).
                HasForeignKey<Pago>(d => d.PedidoId).
                OnDelete(DeleteBehavior.Cascade);

        }
    }
}