using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            // 1. Table name
            builder.ToTable("Clientes");
            // 2. Primary Key
            builder.HasKey(c => c.Id);
            // 3. Propierties
            builder.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Telefono).HasMaxLength(35);
            builder.Property(c => c.Email).HasMaxLength(120);
            builder.Property(c => c.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(c => c.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            // 4. Relations
            //One to Many
            builder.HasMany(c => c.Pedidos).
                WithOne(p => p.Cliente).
                HasForeignKey(p => p.ClienteId);

            builder.HasMany(c => c.Conversaciones).
                WithOne(p => p.Cliente).
                HasForeignKey(p => p.ClienteId);
            /*
            builder.HasMany(c => c.Productos)
                   .WithOne(p => p.Categoria)
                   .HasForeignKey(p => p.CategoriaId);
            */
        }
    }
}