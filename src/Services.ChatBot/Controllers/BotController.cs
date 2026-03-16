using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Webhook.Controllers.Services;

namespace Webhook.Controllers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BotController(IOptions<BotConfiguration> Config) : ControllerBase
{
    [HttpGet("setWebhook")]
    public async Task<string> SetWebHook([FromServices] ITelegramBotClient bot, CancellationToken ct)
    {
        var webhookUrl = Config.Value.BotWebhookUrl.AbsoluteUri;
        await bot.SetWebhook(webhookUrl, allowedUpdates: [], secretToken: Config.Value.SecretToken, cancellationToken: ct);
        return $"Webhook set to {webhookUrl}";
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update,
    [FromServices] ITelegramBotClient bot,
    [FromServices] UpdateHandler handleUpdateService,
    CancellationToken ct)
    {
        // Usamos string.Equals para evitar errores de referencia nula y asegurar comparación segura
        var receivedToken = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
        if (receivedToken != Config.Value.SecretToken)
            return Forbid(); // O return Unauthorized();
        try
        {
            // Procesamiento del mensaje
            await handleUpdateService.HandleUpdateAsync(bot, update, ct);
        }
        catch (Exception exception)
        {
            // Manejo de errores
            await handleUpdateService.HandleErrorAsync(bot, exception, Telegram.Bot.Polling.HandleErrorSource.HandleUpdateError, ct);
        }
        return Ok();
    }
}
