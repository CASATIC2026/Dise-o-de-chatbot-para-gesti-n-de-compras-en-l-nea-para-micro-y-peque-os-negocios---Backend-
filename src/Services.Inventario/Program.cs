using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using Shared.Core; // Referencia a la librería 'sistema circulatorio'
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; // <-- Agrega esta para Swagger
using Microsoft.Extensions.Diagnostics.HealthChecks; // <-- Agrega esta para HealthChecks

var builder = WebApplication.CreateBuilder(args);

// 1. CARGAR CONFIGURACIÓN DE VARIABLES DE ENTORNO (.env)
// Esto asegura que builder.Configuration["JWT_SECRET"] funcione
builder.Configuration.AddEnvironmentVariables();

// 2. CONFIGURACIÓN DE JWT
var jwtSecret = builder.Configuration["JWT_SECRET"];
if (string.IsNullOrEmpty(jwtSecret))
{
    // Fallback por si el .env no carga en local, pero lo ideal es que venga del env
    jwtSecret = "f9a2b8c7e6d5a4b3c2d1e0f9a8b7c6d5e4f3a2b1c0d9e8f7a6b5c4d3e2f1a0b";
}
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
        ValidateLifetime = true // Valida la expiración de 8h que pusiste
    };
});

// 3. SERVICIOS BASE
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Evita problemas de referencias circulares al serializar entidades con relaciones
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// DbContext
builder.Services.AddSharedInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// FluentValidation (Actualizado para evitar warnings de obsolescencia)
builder.Services.AddFluentValidationAutoValidation();

// Swagger y HealthChecks
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// 4. PIPELINE DE MIDDLEWARE
if (app.Environment.IsDevelopment())
{
   
}

app.UseCors("AllowGateway");

// IMPORTANTE: Authentication siempre debe ir ANTES de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();