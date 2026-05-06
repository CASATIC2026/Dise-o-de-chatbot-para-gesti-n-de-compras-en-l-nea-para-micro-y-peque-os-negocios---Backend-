using Services.ChatBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Shared.Core.Entities;
using Services.ChatBot.DTOs;

namespace Webhook.Controllers.Services;

/// <summary>
/// Handles interactive callback queries from Telegram inline buttons.
/// This includes managing product quantities, shopping cart operations, and steering the checkout workflow.
/// </summary>
public class BotInteractionHandler(
IBotPersistencia _persistencia,
BotRenderer renderer,
ICatalogoUI catalogoUI,
PaymentService _paymentService)
{
    /// <summary>
    /// Handles the increment and decrement of product quantities in the product detail view.
    /// Updates the inline keyboard to reflect the new quantity before the user adds the item to the cart.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="parts">The split callback data containing product and category context.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    /// <param name="action">The specific action: "inc" for increment or "dec" for decrement.</param>
    public async Task ManejarCambioCantidad(ITelegramBotClient bot, string[] parts, CallbackQuery callbackQuery, string action)
    {
        int prodId = int.Parse(parts[1]);
        int catId = int.Parse(parts[2]);
        int page = int.Parse(parts[3]);
        var currentMkp = callbackQuery.Message!.ReplyMarkup;

        int currentQty = int.Parse(currentMkp!.InlineKeyboard.ElementAt(0).ElementAt(1).Text);

        if (action == "inc") currentQty++;
        else if (currentQty > 1) currentQty--;

        var keyboard = catalogoUI.BuildUIDetalleProducto(prodId, catId, page, currentQty);

        await bot.EditMessageReplyMarkup(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, keyboard);
    }

    /// <summary>
    /// Triggers the flow for manual quantity entry. 
    /// Updates the message to prompt the user for text input and registers a state in the conversation persistence.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="parts">The split callback data containing product details.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    public async Task ManejarEdicionManual(ITelegramBotClient bot, string[] parts, CallbackQuery callbackQuery)
    {
        int prodId = int.Parse(parts[2]);
        int catId = int.Parse(parts[3]);
        int page = int.Parse(parts[4]);
        int currentQty = int.Parse(parts[5]);

        string instruction = $"*Ingreso Manual*\n\nEscribe la cantidad que deseas para el producto [ID:{prodId}]";
        await bot.AnswerCallbackQuery(callbackQuery.Id, "⌨️ Escribe la cantidad en el chat", showAlert: false);
        Console.WriteLine(instruction);

        var cancelKbd = new InlineKeyboardMarkup(
            InlineKeyboardButton.WithCallbackData("Cancelar", $"prod_{prodId}_{catId}_{page}_{currentQty}")
        );

        //await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, instruction, parseMode: ParseMode.Markdown, replyMarkup: cancelKbd);
        await bot.EditMessageCaption(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, instruction, parseMode: ParseMode.Markdown, replyMarkup: cancelKbd);

        var conv = await _persistencia.ObtenerConversacionActiva(callbackQuery.From.Id);
        if (conv != null)
        {
            await _persistencia.RegistrarMensaje(conv.Id, $" [ID:{prodId}_{catId}_{page}] Esperando cantidad manual...", TipoRemitente.Sistema);
        }
    }

    /// <summary>
    /// Persists a product selection to the user's active shopping cart.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="parts">The split callback data containing product ID and selected quantity.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    public async Task ManejarAgregarAlCarrito(ITelegramBotClient bot, string[] parts, CallbackQuery callbackQuery)
    {
        int prodId = int.Parse(parts[2]);
        int cantidad = int.Parse(parts[3]);
        int catId = int.Parse(parts[4]);
        int page = int.Parse(parts[5]);
        if (cantidad > 0)
        {
            var resultado = await _persistencia.AgregarProducto(callbackQuery.From.Id, prodId, cantidad);
            if (resultado.Success)
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, resultado.msg);
                await renderer.RenderizarCatalogo(bot, callbackQuery, catId, page);
            }
            else
                await bot.AnswerCallbackQuery(callbackQuery.Id, $"Error: {resultado.msg}", showAlert: true);
        }
        else
            await bot.AnswerCallbackQuery(callbackQuery.Id, $"Error: cantidad invalida", showAlert: true);
    }

    /// <summary>
    /// Executes the logic to clear all items from the user's active shopping cart.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    public async Task ManejarVaciarCarrito(ITelegramBotClient bot, CallbackQuery callbackQuery)
    {
        // Lógica para vaciar el carrito (p.ej., eliminar el pedido activo o marcarlo como vacío)                
        if (await _persistencia.VaciarCarrito(callbackQuery.From.Id))
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, "Carrito vaciado. Puedes seguir agregando productos.");
            await renderer.RenderizarCarrito(bot, callbackQuery, callbackQuery.Message!.MessageId);
        }
    }

    /// <summary>
    /// Updates the quantity of an item that is already present in the user's shopping cart.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="parts">The split callback data containing product ID and the new quantity.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    public async Task ManejarEditarItem(ITelegramBotClient bot, string[] parts, CallbackQuery callbackQuery)
    {
        int prodId = int.Parse(parts[2]);
        int cantidad = int.Parse(parts[3]);

        var (Succes, msg) = await _persistencia.ActualizarCantidadCarrito(callbackQuery.From.Id, prodId, cantidad);
        if (Succes)
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, msg, showAlert: true);
            await renderer.RenderizarCarrito(bot, callbackQuery, callbackQuery.Message!.MessageId);
        }
        else
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, $"⚠️ {msg}", showAlert: true);
        }
    }
    /// <summary>
    /// Removes a specific item from the user's shopping cart.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="parts">The split callback data containing the product ID to remove.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    public async Task ManejarEliminarItem(ITelegramBotClient bot, string[] parts, CallbackQuery callbackQuery)
    {
        int prodId = int.Parse(parts[1]);
        var (Succes, msg) = await _persistencia.EliminarItem(callbackQuery.From.Id, prodId);
        if (Succes)
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, msg, showAlert: true);
            await renderer.RenderizarCarrito(bot, callbackQuery, callbackQuery.Message!.MessageId);
        }
        else
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, $"⚠️ {msg}", showAlert: true);
        }

    }

    /// <summary>
    /// Displays a confirmation dialog (inline buttons) to verify if the user wants to empty their entire cart.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    public async Task ManejarAskVaciarCarrito(ITelegramBotClient bot, CallbackQuery callbackQuery)
    {
        var confirmKbd = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("🗑️ SÍ, VACIAR", "clear"),
                InlineKeyboardButton.WithCallbackData("🚫 Cancelar", "cart")
            });
        await bot.EditMessageReplyMarkup(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, confirmKbd);
        await bot.AnswerCallbackQuery(callbackQuery.Id, "🚨 Esto borrará todos los productos.");
    }

    /// <summary>
    /// Displays a confirmation dialog (inline buttons) to verify if the user wants to remove a specific item.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    /// <param name="parts">The split callback data containing the product ID.</param>
    public async Task ManejarAskEliminarItem(ITelegramBotClient bot, CallbackQuery callbackQuery, string[] parts)
    {
        int prodId = int.Parse(parts[2]);

        var confirmKbd = new InlineKeyboardMarkup(new[]
        {
                InlineKeyboardButton.WithCallbackData("✅ Sí", $"rmv_{prodId}"),
                InlineKeyboardButton.WithCallbackData("🚫 Cancelar", "cart")
            });
        await bot.EditMessageReplyMarkup(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, confirmKbd);
        await bot.AnswerCallbackQuery(callbackQuery.Id, "⚠️ ¿Estás seguro de quitar este producto?");
    }
    /// <summary>
    /// Initiates the checkout process by asking the user for their delivery address.
    /// Registers the conversation state so that the next text message is processed as the address.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    public async Task ManejarRegistroDireccionEnvio(ITelegramBotClient bot, CallbackQuery callbackQuery)
    {
        var pedido = await _persistencia.ObtenerPedidoActivo(callbackQuery.From.Id);
        if (pedido == null || !pedido.PedidoProductos.Any())
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, "⚠️ Tu carrito está vacío.", showAlert: true);
            return;
        }
        var conv = await _persistencia.ObtenerConversacionActiva(callbackQuery.From.Id);

        string instruction = "📍 PASO 1: DIRECCIÓN DE ENVÍO\n\nPor favor, escribe tu dirección exacta";
        await _persistencia.RegistrarMensaje(conv!.Id, $" [ESTADO:CHECKOUT_DIRECCION]_Esperando dirección..._", TipoRemitente.Sistema);

        //await bot.EditMessageText(callbackQuerry.Message!.Chat.Id, callbackQuerry.Message.MessageId, instruction, parseMode: ParseMode.Markdown);
        await bot.EditMessageCaption(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, instruction, parseMode: ParseMode.Markdown);
        await bot.AnswerCallbackQuery(callbackQuery.Id);
    }

    /// <summary>
    /// Finalizes the order by updating its status to "Confirmado" and renders the order history to the user.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="callbackQuery">The original callback query from the user interaction.</param>
    public async Task ManejarFinalizacionPedido(ITelegramBotClient bot, CallbackQuery callbackQuery)
    {
        var pedidoactual = await _persistencia.ObtenerPedidoActivo(callbackQuery.From.Id);
        var pedido = new PedidoDTO();
        pedido.Estado = EstadoPedido.Confirmado;
        if (pedidoactual != null)
        {
            pedido.Total = pedidoactual.Total;
            pedido.Id = pedidoactual.Id;
        }
        
        var data = await _paymentService.GeneratedPaymentLink(pedido.Id);
        await renderer.RenderizarTicket(bot, callbackQuery, data!, pedido);
        /*if (data != null)
        {
            //await bot.AnswerCallbackQuery(callbackQuery.Id, msg, showAlert: true);
            //await renderer.RenderizarOrdenes(bot, callbackQuery, 0);                                
        }
        else
        {
            var text = "REINTENTAR";
            var markup = new InlineKeyboardMarkup(new[]{
                InlineKeyboardButton.WithCallbackData(text, "checkout")
            });
            await bot.AnswerCallbackQuery(callbackQuery.Id, $"⚠️ {msg}", showAlert: true);
            //await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, text, parseMode: ParseMode.Markdown, markup);
            await bot.EditMessageCaption(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, text, parseMode: ParseMode.Markdown, markup);
        }*/
    }
}