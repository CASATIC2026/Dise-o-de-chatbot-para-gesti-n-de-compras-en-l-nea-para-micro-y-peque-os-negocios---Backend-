using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    public class PagoConfiguration : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            // 1. Table name
            builder.ToTable("Pagos");
            // 2. Primary Key
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Monto).IsRequired();
            builder.Property(p => p.MetodoPago).IsRequired();
            builder.Property(p => p.Estado).IsRequired();
            builder.Property(p => p.ReferenciaTransaccion).IsRequired();

            builder.Property(p => p.FechaPago).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            // 4. Relations 
            // One to One            
            builder.HasOne(p => p.Pedido).
                WithOne(d => d.Pago).
                HasForeignKey<Pago>(d => d.PedidoId).
                OnDelete(DeleteBehavior.Cascade);

        }
    }
}