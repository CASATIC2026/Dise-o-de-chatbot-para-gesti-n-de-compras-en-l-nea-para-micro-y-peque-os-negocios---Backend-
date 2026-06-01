using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Conversacion"/> entity.
    /// Defines the table structure, primary key, property constraints, and relationships.
    /// </summary>
    public class ConversacionConfiguration : IEntityTypeConfiguration<Conversacion>
    {
        /// <summary>
        /// Configures the entity properties and relationships for <see cref="Conversacion"/>.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Conversacion> builder)
        {
            // 1. Table name
            // Sets the table name to 'Conversaciones' in the database.
            builder.ToTable("Conversaciones");

            // 2. Primary Key
            // Configures the 'Id' property as the unique primary key.
            builder.HasKey(p => p.Id);

            // 3. Properties
            // Configures 'Asunto' with a maximum length of 200 characters.
            builder.Property(p => p.Asunto).HasMaxLength(200);

            // Sets the default value for 'Activa' to true.
            builder.Property(p => p.Activa).HasDefaultValue(true);

            // Configures auditing timestamps to use PostgreSQL's CURRENT_TIMESTAMP by default.
            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 4. Relations 
            // One to Many relationship: A conversation can have multiple messages.
            builder.HasMany(p => p.Mensajes).
                WithOne(p => p.Conversacion).
                HasForeignKey(d => d.ConversacionId);

            // Many to One relationship: A conversation belongs to one client.
            // Cascade delete ensures messages and conversations are cleaned up if a client is removed.
            builder.HasOne(p => p.Cliente).
                WithMany(d => d.Conversaciones).
                HasForeignKey(p => p.ClienteId).
                OnDelete(DeleteBehavior.Cascade);
        }
    }
}