using Microsoft.EntityFrameworkCore;
using Shared.Core.Entities;
using Shared.Core.Mappings;

namespace Shared.Core.Data;

/// <summary>
/// Main application database context.
/// Manages entity sets and database configuration.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Entity sets for database tables.
    /// </summary>
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Conversacion> Conversaciones => Set<Conversacion>();
    public DbSet<Mensaje> Mensajes => Set<Mensaje>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoProducto> PedidoProductos => Set<PedidoProducto>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();


    /// <summary>
    /// Configures the schema needed for the application context.
    /// Applies all entity mappings from the Mappings folder.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente todas las configuraciones
        // que implementen IEntityTypeConfiguration en el ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}