using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Webhook.Controllers.Services;

namespace Webhook.Controllers.Controllers;

/// <summary>
/// API Controller that serves as the main entry point for the Telegram Bot's webhook.
/// Provides endpoints for administrative configuration and processing real-time updates.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BotController(IOptions<BotConfiguration> Config) : ControllerBase
{
    /// <summary>
    /// Configures the Telegram Bot API to send updates to this server's webhook URL.
    /// </summary>
    /// <param name="bot">The injected Telegram bot client.</param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>A confirmation message indicating the result of the webhook registration.</returns>
    [HttpGet("setWebhook")]
    public async Task<string> SetWebHook([FromServices] ITelegramBotClient bot, CancellationToken ct)
    {
        var webhookUrl = Config.Value.BotWebhookUrl.AbsoluteUri;
        await bot.SetWebhook(webhookUrl, allowedUpdates: [], secretToken: Config.Value.SecretToken, cancellationToken: ct);
        return $"Webhook set to {webhookUrl}";
    }

    /// <summary>
    /// Receives incoming <see cref="Update"/> objects from the Telegram Bot API.
    /// Performs security validation via secret token before delegating the update to the <see cref="UpdateHandler"/>.
    /// </summary>
    /// <param name="update">The incoming update from Telegram.</param>
    /// <param name="bot">The injected Telegram bot client.</param>
    /// <param name="handleUpdateService">The service logic for processing different update types.</param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>An <see cref="IActionResult"/> indicating success (200 OK) or failure (403 Forbidden).</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update,
    [FromServices] ITelegramBotClient bot,
    [FromServices] UpdateHandler handleUpdateService,
    CancellationToken ct)
    {
        // Security check: Validate the secret token provided by Telegram to prevent unauthorized requests
        var receivedToken = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
        if (receivedToken != Config.Value.SecretToken)
            return Forbid(); 

        try
        {
            await handleUpdateService.HandleUpdateAsync(bot, update, ct);
        }
        catch (Exception exception)
        {
            // Graceful error handling in case of processing failures
            await handleUpdateService.HandleErrorAsync(bot, exception, Telegram.Bot.Polling.HandleErrorSource.HandleUpdateError, ct);
        }
        return Ok();
    }
}
