namespace Webhook.Controllers;

/// <summary>
/// Represents the configuration settings required to initialize and secure the Telegram Bot.
/// Typically mapped from the "BotConfiguration" section in the application's configuration providers.
/// </summary>
public class BotConfiguration
{
    /// <summary>
    /// Gets the unique authentication token provided by Telegram's BotFather.
    /// </summary>
    public string BotToken { get; init; } = default!;

    /// <summary>
    /// Gets the public absolute URI where the bot will receive webhook updates.
    /// </summary>
    public Uri BotWebhookUrl { get; init; } = default!;

    /// <summary>
    /// Gets the secret token used to verify that incoming requests originate from the Telegram Bot API.
    /// This value is sent in the "X-Telegram-Bot-Api-Secret-Token" header by Telegram.
    /// </summary>
    public string SecretToken { get; init; } = default!;
}
