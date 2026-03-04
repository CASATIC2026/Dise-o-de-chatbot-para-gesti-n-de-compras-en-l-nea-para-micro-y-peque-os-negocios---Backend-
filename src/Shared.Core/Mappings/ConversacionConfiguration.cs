using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    public class ConversacionConfiguration : IEntityTypeConfiguration<Conversacion>
    {
        public void Configure(EntityTypeBuilder<Conversacion> builder)
        {
            // 1. Table name
            builder.ToTable("Conversaciones");
            // 2. Primary Key
            builder.HasKey(p => p.Id);
            // 3. Propierties
            builder.Property(p => p.Asunto).HasMaxLength(200);
            builder.Property(p => p.Activa).HasDefaultValue(true);

            builder.Property(p => p.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            // 4. Relations 
            // One to Many
            builder.HasMany(p => p.Mensajes).
                WithOne(p => p.Conversacion).
                HasForeignKey(d => d.ConversacionId);
            // Many to One
            builder.HasOne(p => p.Cliente).
                WithMany(d => d.Conversaciones).
                HasForeignKey(p => p.ClienteId).
                OnDelete(DeleteBehavior.Cascade);
        }
    }
}