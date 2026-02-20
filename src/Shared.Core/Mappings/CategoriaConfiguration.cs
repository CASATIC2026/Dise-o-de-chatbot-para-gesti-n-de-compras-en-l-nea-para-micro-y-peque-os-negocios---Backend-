using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{

       public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
       {
              public void Configure(EntityTypeBuilder<Categoria> builder)
              {
                     // 1. Table name
                     builder.ToTable("Categorias");

                     // 2. Primary Key
                     builder.HasKey(c => c.Id);

                     // 3. Propierties
                     builder.Property(c => c.Nombre)
                            .IsRequired()
                            .HasMaxLength(50);

                     builder.Property(c => c.Descripcion)
                            .HasMaxLength(200);

                     // 4. Auditoria (PostgreSQL)
                     builder.Property(c => c.CreadoEn)
                            .HasDefaultValueSql("CURRENT_TIMESTAMP");

                     builder.Property(c => c.ActualizadoEn)
                            .HasDefaultValueSql("CURRENT_TIMESTAMP");

                     // 5. Relations One to Many
                     builder.HasMany(c => c.Productos)
                            .WithOne(p => p.Categoria)
                            .HasForeignKey(p => p.CategoriaId);
              }

       }
}