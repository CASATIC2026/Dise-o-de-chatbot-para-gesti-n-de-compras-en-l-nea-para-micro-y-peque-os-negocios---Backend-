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

/// <summary>
/// Entry point for the Inventory Service. 
/// Configures the web host, services, dependency injection, and the request processing pipeline.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <section>
/// Environment Variables Configuration: Loads configuration from system environment variables.
/// </section>
builder.Configuration.AddEnvironmentVariables();

/// <section>
/// Authentication and JWT Configuration: Sets up Bearer authentication with JWT validation logic.
/// </section>
var jwtSecret = builder.Configuration["JWT_SECRET"];
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

    /// <remarks>
    /// SignalR Token Extraction Logic:
    /// Standard WebSockets do not support custom headers in the browser, 
    /// so the JWT must be passed via a query string parameter named 'access_token'.
    /// </remarks>
    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            // Identify if the request is directed to the SignalR notification hub
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

/// <section>
/// Core API Services Configuration: Configures controllers, JSON serialization, and Shared Infrastructure.
/// </section>
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Prevents infinite loops when serializing objects with circular references (e.g., Category -> Products -> Category)
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Add infrastructure from the Shared project (Database, Repositories, etc.)
builder.Services.AddSharedInfrastructure(builder.Configuration);

/// <section>
/// CORS Policy: Configures cross-origin resource sharing specifically for the React frontend.
/// </section>
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Default Vite/React port
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Mandatory for SignalR when using Authentication
    });
});

/// <section>
/// Additional Service Registrations: FluentValidation, SignalR, Swagger, and Health Checks.
/// </section>
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

/// <section>
/// HTTP Request Pipeline: Configures middleware execution order.
/// </section>
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1. Initialize Routing
app.UseRouting();

// 2. CORS must be processed before Authentication to handle preflight (OPTIONS) requests
app.UseCors("AllowGateway");

// 3. Identify who the user is (Authentication) and what they can do (Authorization)
app.UseAuthentication();
app.UseAuthorization();

/// <section>
/// Endpoint Mapping: Maps Controllers, Health Checks, and SignalR Hubs.
/// </section>
app.MapControllers();
app.MapHealthChecks("/health");

// SignalR Hub Route Mapping
app.MapHub<NotificationHub>("/notificationHub");

app.Run();