using Microsoft.EntityFrameworkCore;
using Shared.Core.Data;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSharedInfrastructure(builder.Configuration); 

// Add services to the container
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

// Add Health Checks
/* builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(); */

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
   
}

app.UseCors("AllowGateway");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

app.Run();
