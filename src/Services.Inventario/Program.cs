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
        ValidateLifetime = true
    };

    // Esto permite que SignalR reciba el token por la URL (necesario para WebSockets)
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

// CORS ACTUALIZADO: Para SignalR es vital especificar los orígenes exactos
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Permite cualquier origen (Túnel, Localhost, IP)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Mantener esto para SignalR
    });
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// 4. PIPELINE DE MIDDLEWARE (El orden es CRÍTICO)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1° Routing
app.UseRouting();

// 2° CORS (Debe ir después de Routing y antes de Auth)
app.UseCors("AllowGateway");

// 3° Auth
app.UseAuthentication();
app.UseAuthorization();

// 4° Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

// El Hub DEBE estar después de UseCors y UseAuthorization
app.MapHub<NotificationHub>("/notificationHub");

app.Run();