using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Data; // Namespace donde está tu DbContext

namespace Shared.Core;

public static class DependencyInjection
{
    // Este método es una "extensión" de IServiceCollection (la caja de herramientas de .NET)
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Extraemos la cadena de conexión del appsettings.json del microservicio que nos llame
        // .NET busca automáticamente dentro de la sección "ConnectionStrings"
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Registramos el ApplicationDbContext en el contenedor de Inyección de Dependencias
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b =>
                // IMPORTANTE: Le decimos a EF Core que las migraciones (tablas) 
                // están definidas aquí mismo, en el proyecto Shared.Core
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Retornamos los servicios para permitir el encadenamiento (Fluent API)
        return services;
    }
}
