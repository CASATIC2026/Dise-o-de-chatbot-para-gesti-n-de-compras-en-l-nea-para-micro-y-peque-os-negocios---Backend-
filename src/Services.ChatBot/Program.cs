using Telegram.Bot;
using Webhook.Controllers;
using Services.ChatBot.Interfaces;
using Services.ChatBot.Models;
using Webhook.Controllers.Controllers;
using FluentValidation.AspNetCore;
using Shared.Core.Data;
using Shared.Core;
var builder = WebApplication.CreateBuilder(args);

// Setup bot configuration
var botConfigSection = builder.Configuration.GetSection("BotConfiguration");
builder.Services.Configure<BotConfiguration>(botConfigSection);
builder.Configuration.AddEnvironmentVariables();


builder.Services.AddHttpClient("tgwebhook").
//RemoveAllLoggers().
AddTypedClient<ITelegramBotClient>(
    httpClient => new TelegramBotClient(botConfigSection.Get<BotConfiguration>()!.BotToken, httpClient));

builder.Services.AddHttpClient("GatewayApi", client =>
{
    var gatewayUrl = builder.Configuration["GatewaySettings:Url"];
    client.BaseAddress = new Uri(gatewayUrl);
}
);
builder.Services.AddScoped<IMenuUI, CategoriasModule>();
builder.Services.AddScoped<ICatalogoUI, ProductosModule>();
builder.Services.AddScoped<Webhook.Controllers.Services.UpdateHandler>();
builder.Services.AddScoped<IUtilsUI, UtilsModule>();
builder.Services.AddScoped<IBotPersistencia, SqlBotPersistence>();
builder.Services.AddControllers();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowGateway");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
