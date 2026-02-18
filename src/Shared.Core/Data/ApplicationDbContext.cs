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

        // Producto Configuration
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Precio).HasPrecision(10, 2);
            entity.Property(e => e.CreadoEn).HasDefaultValueSql("NOW()");
            entity.Property(e => e.ActualizadoEn).HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Activo);
        });

        // Usuario Configuration
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            // Configure JSONB column for PostgreSQL
            entity.Property(e => e.HistorialConversacion)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.Property(e => e.CreadoEn).HasDefaultValueSql("NOW()");
            entity.Property(e => e.ActualizadoEn).HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.TelegramId).IsUnique();
            entity.HasIndex(e => e.WhatsAppId).IsUnique();
        });

        // Pedido Configuration
        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Total).HasPrecision(10, 2);
            entity.Property(e => e.DireccionEntrega).IsRequired().HasMaxLength(500);

            // Configure JSONB column for order details
            entity.Property(e => e.DetallesJson)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.Property(e => e.CreadoEn).HasDefaultValueSql("NOW()");
            entity.Property(e => e.ActualizadoEn).HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.ReferenciaWompi);

            // Relationship configuration
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
