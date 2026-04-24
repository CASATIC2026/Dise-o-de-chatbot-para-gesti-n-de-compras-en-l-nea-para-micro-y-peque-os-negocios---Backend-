using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{

    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            //1.  Table name
            builder.ToTable("Usuarios");
            //2. Primary Key
            builder.HasKey(u => u.Id);

            //3. Propierties
            builder.Property(u => u.Nombre).IsRequired().HasDefaultValue("");
            builder.Property(u => u.Email);
            builder.Property(u => u.ContrasenaHash).HasMaxLength(500).IsRequired();
            builder.Property(u => u.Telefono);
            builder.Property(u => u.Rol).IsRequired().HasDefaultValue(Roles.Vendedor);
            builder.Property(u => u.Estado).IsRequired().HasDefaultValue(true);

            //4. Auditoria (PostgreSQL)
            builder.Property(u => u.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(u => u.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");

            //Relation One to Many
            builder.HasMany(u => u.Pedidos).
                    WithOne(d => d.Usuario).
                    HasForeignKey(p => p.UsuarioId).
                    OnDelete(DeleteBehavior.Cascade);

        }
    }
}
