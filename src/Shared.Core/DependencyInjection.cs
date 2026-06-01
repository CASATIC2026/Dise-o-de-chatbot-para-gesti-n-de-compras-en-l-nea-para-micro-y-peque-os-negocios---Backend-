using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Data;

namespace Shared.Core;

/// <summary>
/// Contains extension methods for registering shared core infrastructure in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds shared infrastructure services, such as the PostgreSQL database context, to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to retrieve settings, such as connection strings, from.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Extracts the connection string from the 'ConnectionStrings' section of the configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Configures and registers the ApplicationDbContext using Npgsql (PostgreSQL)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b =>
                // Specifies that migrations for this context are located in the Shared.Core assembly
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        return services;
    }
}