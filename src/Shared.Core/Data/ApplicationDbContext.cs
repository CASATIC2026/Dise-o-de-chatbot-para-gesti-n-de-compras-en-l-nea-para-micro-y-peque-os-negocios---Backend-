using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;
using Shared.Core.Mappings;
using Microsoft.EntityFrameworkCore.Metadata;

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
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Forzar el esquema ecommerce (Esto está perfecto)
    modelBuilder.HasDefaultSchema("ecommerce");

    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        // 1. Obtener el nombre de la tabla definido en la clase (vía [Table] o por defecto)
        var currentTableName = entity.GetTableName();

        // 2. Solo forzamos a minúsculas si NO hemos definido un nombre manual 
        // para evitar que 'usuarios' se convierta en algo raro.
        entity.SetTableName(currentTableName?.ToLower());

        // 3. Forzar nombres de columnas a minúsculas para que coincidan con el SQL
        foreach (var property in entity.GetProperties())
        {
            var storeObjectIdentifier = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            var currentColumnName = property.GetColumnName(storeObjectIdentifier);
            property.SetColumnName(currentColumnName?.ToLower());
        }
    }
}
}