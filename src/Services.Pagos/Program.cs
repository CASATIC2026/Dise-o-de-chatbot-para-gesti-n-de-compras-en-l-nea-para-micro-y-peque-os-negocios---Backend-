using Microsoft.EntityFrameworkCore;
using Shared.Core;
using Services.Pagos.Services;

namespace Services.Pagos;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configurar puerto para Docker
        builder.WebHost.UseUrls("http://0.0.0.0:8080");

        // Servicios
        builder.Services.AddHttpClient<WompiService>();
        builder.Services.AddSharedInfrastructure(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpClient();
        builder.Services.AddHostedService<PagoTimeoutWorker>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<Shared.Core.Data.ApplicationDbContext>();

                Console.WriteLine("🔍 [SISTEMA]: Verificando conexión a PostgreSQL...");
                var connectionString = context.Database.GetDbConnection().ConnectionString;
                Console.WriteLine($"DEBUG: Connecting to {connectionString.Split(';')[0]} with SSL...");

                if (await context.Database.CanConnectAsync())
                {
                    Console.WriteLine("✅ [CONEXIÓN EXITOSA]: El microservicio de Pagos está conectado a la DB.");

                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                    if (pendingMigrations.Any())
                    {
                        Console.WriteLine("⚙️ Aplicando migraciones...");
                        await context.Database.MigrateAsync();
                        Console.WriteLine("✅ Migraciones aplicadas correctamente.");
                    }
                    else
                    {
                        Console.WriteLine("ℹ️ No hay migraciones pendientes.");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ No se pudo conectar a PostgreSQL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ERROR CRÍTICO]: Fallo al inicializar la DB: {ex.Message}");
            }
        }

        // Middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
