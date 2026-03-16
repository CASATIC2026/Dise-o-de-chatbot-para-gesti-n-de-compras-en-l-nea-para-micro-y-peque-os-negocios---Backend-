using Microsoft.EntityFrameworkCore;
using Shared.Core; // Referencia a la librería 'sistema circulatorio'using Swashbuckle.AspNetCore;
using Services.Pagos.Services;

namespace Services.Pagos;

public class Program
{
    // Cambio: de 'static void Main' a 'static async Task Main'
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ... tus registros de servicios ...
        builder.WebHost.UseUrls("http://0.0.0.0:8080");
        builder.Services.AddHttpClient<WompiService>();
        builder.Services.AddSharedInfrastructure(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // --- PRUEBA DE CONEXIÓN EN CALIENTE ---
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<Shared.Core.Data.ApplicationDbContext>();

                Console.WriteLine("🔍 [SISTEMA]: Verificando conexión a PostgreSQL...");

                // Ahora el await funcionará correctamente
                if (await context.Database.CanConnectAsync())
                {
                    Console.WriteLine("✅ [CONEXIÓN EXITOSA]: El microservicio de Inventario está conectado a la DB.");
                }
                else
                {
                    Console.WriteLine("⚠️ [ADVERTENCIA]: No se pudo establecer contacto con PostgreSQL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ERROR CRÍTICO]: Fallo al inyectar o conectar la DB: {ex.Message}");
            }
        }
        // Esto aplicará cualquier migración pendiente al arrancar el contenedor
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Shared.Core.Data.ApplicationDbContext>();

            context.Database.Migrate();
        }
        // Aplicar migraciones automáticamente al iniciar
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<Shared.Core.Data.ApplicationDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                    Console.WriteLine("✅ Migraciones aplicadas con éxito.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al aplicar migraciones: {ex.Message}");
            }
        }

        // ... resto del pipeline ...
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