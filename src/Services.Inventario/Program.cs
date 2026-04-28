using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Service.Inventario.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE VARIABLES DE ENTORNO
builder.Configuration.AddEnvironmentVariables();

// 2. CONFIGURACIÓN DE JWT
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "f9a2b8c7e6d5a4b3c2d1e0f9a8b7c6d5e4f3a2b1c0d9e8f7a6b5c4d3e2f1a0b";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Opcional: elimina el margen de 5 min para expirar tokens
    };

    // VITAL: Configuración para que SignalR lea el token de la Query String
    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// 3. SERVICIOS BASE
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddSharedInfrastructure(builder.Configuration);

// CORS LOCAL: Configuración específica para desarrollo local con React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Tu puerto de React
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Obligatorio para SignalR con Auth
    });
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// 4. PIPELINE DE MIDDLEWARE (El orden es la clave del éxito)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1. Routing debe ser lo primero
app.UseRouting();

// 2. CORS DEBE ir antes de Authentication
app.UseCors("AllowGateway");

// 3. Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// 4. Endpoints (Controllers y Hubs)
app.MapControllers();
app.MapHealthChecks("/health");

// Mapeo del Hub para SignalR
app.MapHub<NotificationHub>("/notificationHub");

app.Run();