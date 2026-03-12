using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    public class PedidoProductoConfiguration : IEntityTypeConfiguration<PedidoProducto>
    {
        public void Configure(EntityTypeBuilder<PedidoProducto> builder)
        {
            // 1. Table name
            builder.ToTable("PedidoProductos");
            // 2. Primary Key
            builder.HasKey(p => p.Id);
            // 3. Propierties
            builder.Property(p => p.Cantidad).IsRequired();
            builder.Property(p => p.PrecioUnitario).IsRequired();
            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 4. Relations 
            // Many to One
            builder.HasOne(p => p.Producto).
                WithMany(d => d.PedidoProductos).
                HasForeignKey(p => p.ProductoId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(p => p.Pedido).
                WithMany(d => d.PedidoProductos).
                HasForeignKey(p => p.PedidoId).OnDelete(DeleteBehavior.Cascade);


        }
    }
}