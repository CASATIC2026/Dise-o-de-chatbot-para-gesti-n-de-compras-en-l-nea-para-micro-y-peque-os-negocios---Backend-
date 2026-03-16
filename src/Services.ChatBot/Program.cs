using Telegram.Bot;
using Webhook.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Setup bot configuration
var botConfigSection = builder.Configuration.GetSection("BotConfiguration");
builder.Services.Configure<BotConfiguration>(botConfigSection);

builder.Services.AddHttpClient("tgwebhook").
RemoveAllLoggers().
AddTypedClient<ITelegramBotClient>(
    httpClient => new TelegramBotClient(botConfigSection.Get<BotConfiguration>()!.BotToken, httpClient));

builder.Services.AddHttpClient("GatewayApi", client =>
{
    var gatewayUrl = builder.Configuration["GatewaySettings:Url"];
    client.BaseAddress = new Uri(gatewayUrl);
}
);

builder.Services.AddScoped<Webhook.Controllers.Services.UpdateHandler>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
