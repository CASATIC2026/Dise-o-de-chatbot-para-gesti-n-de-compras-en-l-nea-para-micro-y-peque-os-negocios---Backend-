using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Mensaje"/> entity.
    /// Defines table name, primary key, property constraints, and relationships.
    /// </summary>
    public class MensajeConfiguration : IEntityTypeConfiguration<Mensaje>
    {
        /// <summary>
        /// Configures the entity properties and relationships for <see cref="Mensaje"/>.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Mensaje> builder)
        {
            // 1. Table name
            // Sets the table name for the Mensaje entity in the database.
            builder.ToTable("Mensajes");

            // 2. Primary Key
            // Configures the 'Id' property as the unique primary key.
            builder.HasKey(p => p.Id);

            // 3. Properties
            // Configures 'Contenido' with a maximum length of 2000 characters.
            builder.Property(p => p.Contenido).HasMaxLength(2000);

            // Configures 'Remitente' as a required field.
            builder.Property(p => p.Remitente).IsRequired();

            // Configures 'FechaEnvio' to use PostgreSQL's CURRENT_TIMESTAMP by default.
            builder.Property(p => p.FechaEnvio).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 4. Relations 
            // Many to One
            // Configures a many-to-one relationship: many messages belong to one conversation.
            // Cascade delete ensures messages are removed if the associated conversation is deleted.
            builder.HasOne(p => p.Conversacion).
                WithMany(p => p.Mensajes).
                HasForeignKey(d => d.ConversacionId).
                OnDelete(DeleteBehavior.Cascade);

        }
    }
}