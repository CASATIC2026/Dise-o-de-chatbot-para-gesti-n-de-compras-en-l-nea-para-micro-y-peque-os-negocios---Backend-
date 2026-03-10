using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        // Tabla
        builder.ToTable("usuarios");

        // Clave primaria
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id_usuario")
            .ValueGeneratedOnAdd();

        // Propiedades básicas
        builder.Property(u => u.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.ContrasenaHash)
            .HasColumnName("contrasena_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.Rol)
            .HasColumnName("rol")
            .HasMaxLength(50);

        builder.Property(u => u.Estado)
            .HasColumnName("estado")
            .HasDefaultValue(true);

        // Integraciones
        builder.Property(u => u.TelegramId)
            .HasColumnName("telegram_id");

        builder.Property(u => u.WhatsAppId)
            .HasColumnName("whatsapp_id")
            .HasMaxLength(50);

        builder.Property(u => u.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(25);

        // JSONB para historial de conversación
        builder.Property(u => u.HistorialConversacion)
            .HasColumnName("historial_conversacion")
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");

        // Auditoría
        builder.Property(u => u.CreadoEn)
            .HasColumnName("creado_en")
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.ActualizadoEn)
            .HasColumnName("actualizado_en")
            .HasDefaultValueSql("NOW()");

        // Relaciones
        builder.HasMany(u => u.Pedidos)
            .WithOne(p => p.Usuario)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}