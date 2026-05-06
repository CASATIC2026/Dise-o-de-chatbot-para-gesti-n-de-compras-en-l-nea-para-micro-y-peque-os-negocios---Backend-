using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Webhook.Controllers.Services;
using Services.ChatBot.DTOs;
using Shared.Core.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
    [HttpPost("pago-procesando")]
    public async Task<IActionResult> NotificarPagoProcesando(
        [FromServices] ITelegramBotClient bot,
        [FromServices] ApplicationDbContext db,
        [FromBody] NotificacionPagosDTO notificacion
    )
    {
        var pedido = await db.Pedidos.Include(p => p.Cliente).FirstOrDefaultAsync(p => p.ReferenciaWompi == notificacion.Referencia);
        //if (pedido == null && pedido!.Cliente == null) return NotFound();
        try
        {
            var telegramId = pedido!.Cliente!.TelegramId;
            var lastconversacion = await db.Conversaciones.FirstOrDefaultAsync(c => c.ClienteId == pedido.ClienteId && c.Activa == true);
            //if (lastconversacion == null) return NotFound();

            string Url = notificacion.Url;
            if (string.IsNullOrEmpty(Url)) return NotFound();

            string urlCodec = Uri.EscapeDataString(Url);
            string urlPublic = "adele-unconvergent-preternaturally.ngrok-free.dev/api/pagos/redirect";

            int.TryParse(lastconversacion!.Asunto, out int msgId);
            string url = $"{urlPublic}?url={urlCodec}&convasacionId={msgId}&refe={""}";

            var keyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl("➡️ IR A PAGAR", url)
            );

            string telegramIdString = telegramId.ToString()!;
            long telegramIdLong = long.Parse(telegramIdString);

            string text = $"En proceso de pago del Pedido \\#{pedido.Id}\nRecibira un msg cuando el pago sea procesado";

            CallbackQuery cq = new()
            {
                Data = "menu",
                From = new User { Id = telegramIdLong },
                Message = new Message
                {
                    Chat = new Chat
                    {
                        Id = telegramIdLong,
                        Type = ChatType.Private
                    },
                }
            };

            Console.WriteLine("Msg Id en pago-proceso: " + msgId);
            await bot.EditMessageCaption(cq.Message!.Chat, msgId, text, parseMode: ParseMode.MarkdownV2, replyMarkup: keyboard);

            pedido.Estado = Shared.Core.Entities.EstadoPedido.Confirmado;
            pedido.ActualizadoEn = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex + "Error al enviar notificacion de pago realizado");
            return NotFound();
        }
    }
    [HttpPost("pagos-completado")]
    public async Task<IActionResult> NotificarPagoRealizado([FromServices] ITelegramBotClient bot,
    [FromServices] ApplicationDbContext db,
    [FromBody] NotificacionPagosDTO notificacion)
    {
        var pedido = await db.Pedidos.Include(p => p.Cliente).FirstOrDefaultAsync(p => p.ReferenciaWompi == notificacion.Referencia);
        if (pedido == null) return NotFound();
        try
        {
            try
            {
                var telegramId = pedido!.Cliente!.TelegramId;
                var lastconversacion = await db.Conversaciones.FirstOrDefaultAsync(c => c.ClienteId == pedido.ClienteId && c.Activa == true);
                //if (lastconversacion == null) return NotFound();            

                int.TryParse(lastconversacion!.Asunto, out int msgId);                                

                string telegramIdString = telegramId.ToString()!;
                long telegramIdLong = long.Parse(telegramIdString);

                string text = $"Pago del Pedido {pedido.Id} recibido, en proceso de envio";

                CallbackQuery cq = new()
                {
                    Data = "menu",
                    From = new User { Id = telegramIdLong },
                    Message = new Message
                    {
                        Chat = new Chat
                        {
                            Id = telegramIdLong,
                            Type = ChatType.Private
                        },
                    }
                };

                Console.WriteLine("Msg Id en pago-proceso: " + msgId);
                await bot.EditMessageCaption(cq.Message!.Chat, msgId, text, parseMode: ParseMode.MarkdownV2);                

                return Ok();
            }
            catch
            {
                await bot.SendMessage(pedido!.Cliente!.TelegramId!, $"Pago del Pedido {pedido.Id} recibido, en proceso de envio");
                return Ok();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex + "Error al enviar notificacion de pago realizado");
            return NotFound();
        }
    }
}
