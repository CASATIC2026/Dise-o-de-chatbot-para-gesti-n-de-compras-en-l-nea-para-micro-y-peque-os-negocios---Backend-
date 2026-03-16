using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            //1.  Table name
            builder.ToTable("Productos");
            //2. Primary Key
            builder.HasKey(p => p.Id);
            //3. Propierties
            builder.Property(p => p.Nombre).HasMaxLength(50);
            builder.Property(p => p.Descripcion).HasMaxLength(200);
            builder.Property(p => p.Precio).IsRequired();
            builder.Property(p => p.Stock).IsRequired();
            builder.Property(p => p.ImagenUrl).IsRequired();
            builder.Property(p => p.Activo).HasDefaultValue(true);
            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            //4. Relations
            // Many to One
            builder.HasMany(p => p.PedidoProductos).
                   WithOne(d => d.Producto).
                   HasForeignKey(p => p.ProductoId);
            // One to Many
            builder.HasOne(p => p.Categoria).
                    WithMany(d => d.Productos).
                    HasForeignKey(p => p.CategoriaId).
                    OnDelete(DeleteBehavior.Cascade);
        }
    }
}