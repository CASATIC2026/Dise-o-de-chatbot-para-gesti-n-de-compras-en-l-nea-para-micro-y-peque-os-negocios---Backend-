using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Usuario"/> entity.
    /// Defines table name, primary key, property constraints, and relationships.
    /// </summary>
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        /// <summary>
        /// Configures the entity properties and relationships for <see cref="Usuario"/>.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            // 1. Table name
            // Sets the table name for the Usuario entity in the database.
            builder.ToTable("Usuarios");

            // 2. Primary Key
            // Configures the 'Id' property as the unique primary key.
            builder.HasKey(u => u.Id);

            // 3. Properties
            // Configures 'Nombre' as a required field with a default empty string.
            builder.Property(u => u.Nombre).IsRequired().HasDefaultValue("");

            // Email property configuration.
            builder.Property(u => u.Email);

            // Configures 'ContrasenaHash' as a required field with a maximum length of 500 characters.
            builder.Property(u => u.ContrasenaHash).HasMaxLength(500).IsRequired();

            // Telefono property configuration.
            builder.Property(u => u.Telefono);

            // Configures 'Rol' with a default value of 'Vendedor'.
            builder.Property(u => u.Rol).IsRequired().HasDefaultValue(Roles.Vendedor);

            // Sets the default active status to true.
            builder.Property(u => u.Estado).IsRequired().HasDefaultValue(true);

            // 4. Auditoria (PostgreSQL)
            // Configures auditing timestamps to use PostgreSQL's CURRENT_TIMESTAMP by default.
            builder.Property(u => u.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(u => u.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 5. Relation One to Many
            // Configures a one-to-many relationship: one user (staff) can manage multiple orders.
            // Cascade delete ensures orders associated with a user are handled if the user is removed.
            builder.HasMany(u => u.Pedidos).
                    WithOne(d => d.Usuario).
                    HasForeignKey(p => p.UsuarioId).
                    OnDelete(DeleteBehavior.Cascade);

        }
    }
}
