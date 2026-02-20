using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
    public class MensajeConfiguration : IEntityTypeConfiguration<Mensaje>
    {
        public void Configure(EntityTypeBuilder<Mensaje> builder)
        {
            // 1. Table name
            builder.ToTable("Mensajes");
            // 2. Primary Key
            builder.HasKey(p => p.Id);
            // 3. Propierties
            builder.Property(p => p.Contenido).HasMaxLength(2000);
            builder.Property(p => p.Remitente).IsRequired();
            builder.Property(p => p.FechaEnvio).HasDefaultValueSql("CURRENT_TIMESTAMP");
            // 4. Relations 
            // Many to One
            builder.HasOne(p => p.Conversacion).
                WithMany(p => p.Mensajes).
                HasForeignKey(d => d.ConversacionId).
                OnDelete(DeleteBehavior.Cascade);

        }
    }
}