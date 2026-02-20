using Microsoft.EntityFrameworkCore;
using Shared.Core.Entities;

namespace Shared.Core.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===============================
        // PRODUCTO
        // ===============================
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("productos");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id_producto");

            entity.Property(e => e.Nombre)
                  .IsRequired()
                  .HasMaxLength(200)
                  .HasColumnName("nombre");

            entity.Property(e => e.Descripcion)
                  .HasMaxLength(1000)
                  .HasColumnName("descripcion");

            entity.Property(e => e.Precio)
                  .HasPrecision(10, 2)
                  .HasColumnName("precio");

            entity.Property(e => e.Stock)
                  .HasColumnName("stock");

            entity.Property(e => e.Activo)
                  .HasColumnName("activo");

            entity.Property(e => e.CreadoEn)
                  .HasColumnName("creado_en")
                  .HasDefaultValueSql("NOW()");

            entity.Property(e => e.ActualizadoEn)
                  .HasColumnName("actualizado_en")
                  .HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Activo);
        });

        // ===============================
        // USUARIO
        // ===============================
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id_usuario");

            entity.Property(e => e.Nombre)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasColumnName("nombre");

            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(150)
                  .HasColumnName("email");

            entity.Property(e => e.ContrasenaHash)
                  .IsRequired()
                  .HasColumnName("contrasena_hash");

            entity.Property(e => e.Rol)
                  .HasMaxLength(50)
                  .HasColumnName("rol");

            entity.Property(e => e.Estado)
                  .HasColumnName("estado");

            entity.Property(e => e.Telefono)
                  .HasMaxLength(20)
                  .HasColumnName("telefono");

            entity.Property(e => e.TelegramId)
                  .HasColumnName("telegram_id");

            entity.Property(e => e.WhatsAppId)
                  .HasColumnName("whatsapp_id");

            entity.Property(e => e.HistorialConversacion)
                  .HasColumnName("historial_conversacion")
                  .HasColumnType("jsonb")
                  .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.CreadoEn)
                  .HasColumnName("creado_en")
                  .HasDefaultValueSql("NOW()");

            entity.Property(e => e.ActualizadoEn)
                  .HasColumnName("actualizado_en")
                  .HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.TelegramId).IsUnique();
            entity.HasIndex(e => e.WhatsAppId).IsUnique();
        });

        // ===============================
        // PEDIDO
        // ===============================
        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("pedido");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id_pedido");

            entity.Property(e => e.UsuarioId)
                  .HasColumnName("id_usuario");

            entity.Property(e => e.Total)
                  .HasPrecision(10, 2)
                  .HasColumnName("total");

            entity.Property(e => e.Estado)
                  .HasMaxLength(50)
                  .HasColumnName("estado");

            entity.Property(e => e.DireccionEntrega)
                  .IsRequired()
                  .HasMaxLength(500)
                  .HasColumnName("direccion_entrega");

            entity.Property(e => e.ReferenciaWompi)
                  .HasColumnName("referencia_wompi");

            entity.Property(e => e.DetallesJson)
                  .HasColumnName("detalles_json")
                  .HasColumnType("jsonb")
                  .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.CreadoEn)
                  .HasColumnName("creado_en")
                  .HasDefaultValueSql("NOW()");

            entity.Property(e => e.ActualizadoEn)
                  .HasColumnName("actualizado_en")
                  .HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.ReferenciaWompi);

            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Pedidos)
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}