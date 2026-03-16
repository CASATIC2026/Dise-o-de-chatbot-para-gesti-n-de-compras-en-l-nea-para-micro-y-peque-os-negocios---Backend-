using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
    {
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            // 1. Table name
            builder.ToTable("Pedidos");
            // 2. Primary Key
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Estado).HasDefaultValue(EstadoPedido.Pendiente);
            builder.Property(p => p.Total).IsRequired();
            builder.Property(p => p.DireccionEntrega).HasMaxLength(200).HasDefaultValue("");
            builder.Property(p => p.DetallesJson).IsRequired();
            builder.Property(p => p.ReferenciaWompi);
            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 4. Relations 
            // One to One
            builder.HasOne(p => p.Pago).
                WithOne(d => d.Pedido).
                HasForeignKey<Pago>(d => d.PedidoId).
                OnDelete(DeleteBehavior.Cascade);
            // Many to One
            builder.HasOne(p => p.Cliente).
                WithMany(d => d.Pedidos).
                HasForeignKey(p => p.ClienteId).
                OnDelete(DeleteBehavior.Cascade);
            // Many to One
            builder.HasOne(p => p.Usuario).
                WithMany(d => d.Pedidos).
                HasForeignKey(p => p.UsuarioId).
                OnDelete(DeleteBehavior.Cascade);
            // One to Many
            builder.HasMany(p => p.PedidoProductos).
                WithOne(p => p.Pedido).
                HasForeignKey(d => d.PedidoId);
        }
    }
}

