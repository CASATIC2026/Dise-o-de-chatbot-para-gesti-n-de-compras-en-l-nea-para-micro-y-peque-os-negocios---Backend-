using Telegram.Bot;
using Webhook.Controllers;
using Services.ChatBot.Interfaces;
using Services.ChatBot.Models;
using Webhook.Controllers.Controllers;
using FluentValidation.AspNetCore;
using Shared.Core.Data;
using Shared.Core;
using Services.ChatBot.Utils;
using Webhook.Controllers.Services;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

// --- Bot Configuration ---
// Binds the "BotConfiguration" section from appsettings.json and enables environment variables.
var botConfigSection = builder.Configuration.GetSection("BotConfiguration");
builder.Services.Configure<BotConfiguration>(botConfigSection);
builder.Configuration.AddEnvironmentVariables();

// --- HttpClient Registrations ---
// Configures the Telegram Bot Client with a typed HttpClient.
builder.Services.AddHttpClient("tgwebhook").
AddTypedClient<ITelegramBotClient>(
    httpClient => new TelegramBotClient(botConfigSection.Get<BotConfiguration>()!.BotToken, httpClient));

// Configures the client for the internal Gateway/Inventory API.
builder.Services.AddHttpClient("GatewayApi", client =>
{
    var gatewayUrl = builder.Configuration["GatewaySettings:Url"] ?? "http://localhost:3000";
    client.BaseAddress = new Uri(gatewayUrl);
}
);
// Configures the client for the internal Pagos API.
builder.Services.AddHttpClient("PagosApi", client =>
{
    var pagosUrl = builder.Configuration["PagosSettings:Url"] ?? "http//localhost:5002"; 
    client.BaseAddress = new Uri(pagosUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
}
);

// --- Dependency Injection: UI Modules and Services ---
builder.Services.AddScoped<IMenuUI, MenuModule>();
builder.Services.AddScoped<ICatalogoUI, CatalogoModule>();
builder.Services.AddScoped<ICarrito, CarritoModule>();
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<IUtilsUI, UtilsModule>();
builder.Services.AddScoped<IBotPersistencia, SqlBotPersistence>();
builder.Services.AddScoped<BotRenderer>();
builder.Services.AddScoped<BotInteractionHandler>();
builder.Services.AddScoped<BotOnMsgInteractionHandler>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddControllers();

// --- Background Workers ---
// Periodically cleans up stock from abandoned/expired carts.
builder.Services.AddHostedService<StockReleaseWorker>();

// --- Infrastructure ---
builder.Services.AddSharedInfrastructure(builder.Configuration);

// --- Global Policies ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- Features: Validation, Documentation, and Health ---
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// --- HTTP Request Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowGateway");
app.UseHttpsRedirection();
app.UseAuthorization();

// --- Routing ---
app.MapControllers();
app.MapHealthChecks("/health");

// --- Launch ---
app.Run();
