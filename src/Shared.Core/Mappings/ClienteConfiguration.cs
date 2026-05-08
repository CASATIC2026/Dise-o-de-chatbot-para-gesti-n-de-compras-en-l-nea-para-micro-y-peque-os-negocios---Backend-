using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    /// <summary>
    /// Configures the database mapping for the <see cref="Cliente"/> entity.
    /// Defines table names, constraints, and relationships with orders and conversations.
    /// </summary>
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        /// <summary>
        /// Configures the entity properties and relationships for <see cref="Cliente"/>.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            // 1. Table name
            // Sets the table name to 'Clientes' in the database.
            builder.ToTable("Clientes");

            // 2. Primary Key
            // Configures the 'Id' property as the unique primary key.
            builder.HasKey(c => c.Id);

            // 3. Propierties
            // Configures 'Nombre' as a required field with a maximum length of 200 characters.
            builder.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
            
            // Sets specific maximum lengths for contact details.
            builder.Property(c => c.Telefono).HasMaxLength(35);
            builder.Property(c => c.Email).HasMaxLength(120);
            
            // Configures auditing timestamps to use PostgreSQL's CURRENT_TIMESTAMP by default.
            builder.Property(c => c.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(c => c.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Identifiers for messaging platforms.
            builder.Property(c => c.TelegramId);
            builder.Property(c => c.WhatsAppId);
            
            // Configures the conversation history column as a JSONB type for efficient storage and querying in PostgreSQL.
            builder.Property(c => c.HistorialConversacion).HasColumnType("jsonb");

            // 4. Relations
            // One to Many relationship: A client can have multiple orders.
            builder.HasMany(c => c.Pedidos).
                WithOne(p => p.Cliente).
                HasForeignKey(p => p.ClienteId);

            // One to Many relationship: A client can have multiple conversation records.
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