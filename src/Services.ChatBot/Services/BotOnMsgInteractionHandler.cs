using Services.ChatBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Shared.Core.Entities;
using Services.ChatBot.DTOs;

namespace Webhook.Controllers.Services
{
    /// <summary>
    /// Handles message interactions for the chatbot, including conversation initialization,
    /// product quantity editing, and checkout workflow processing.
    /// </summary>
    public class BotOnMsgInteractionHandler(
    IBotPersistencia _persistencia,
    BotRenderer renderer)
    {
        private readonly string url = "https://placehold.co/360x100/png?text=Tienda";

        /// <summary>
        /// Handles the initial message of a conversation by displaying the main menu.
        /// </summary>
        /// <param name="bot">The Telegram bot client.</param>
        /// <param name="msg">The message from the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ManejoMsgInicioConversacion(ITelegramBotClient bot, Message msg)
        {

            CallbackQuery callbackQuerry = new()
            {
                Data = "menu",
                Message = new Message
                {
                    Chat = msg.Chat
                }
            };
            await renderer.RenderizarMenu(bot, msg, callbackQuerry);
            return;
        }

        /// <summary>
        /// Handles manual product quantity editing when a user enters a number in response to a product display.
        /// Parses the product ID and category from the last system message and renders the product with the specified quantity.
        /// </summary>
        /// <param name="bot">The Telegram bot client.</param>
        /// <param name="msg">The message from the user containing the quantity.</param>
        /// <param name="text">The text content of the message (the quantity as a string).</param>
        /// <returns>True if the message was successfully processed, otherwise false.</returns>
        public async Task<bool> ManejoMsgEdicionManualCantProduc(ITelegramBotClient bot, Message msg, string text)
        {
            var conv = await _persistencia.ObtenerConversacionActiva(msg.From!.Id);
            if (conv == null) return false;
            var lastMsg = await _persistencia.ObtenerUltimoMensaje(conv.Id);
            if (lastMsg != null && lastMsg.Remitente == TipoRemitente.Sistema && lastMsg.Contenido.Contains("[ID:"))
            {
                if (int.TryParse(text, out int cantidad) && cantidad > 0)
                {
                    string fragmento = lastMsg.Contenido.Split('[', ']')[1]; // "ID:2_3_0"
                    string[] partes = fragmento.Split(':')[1].Split('_');    // ["2", "3", "0"]

                    int prodId = int.Parse(partes[0]);
                    int catId = int.Parse(partes[1]);
                    int page = int.Parse(partes[2]);
                    Console.WriteLine($"prodId {prodId}, catId {catId}, page {page}");

                    string data = (catId == -1) ? $"prod_{prodId}_{-1}_{0}" : $"prod_{prodId}_{catId}_{page}";

                    CallbackQuery callbackQuery = new()
                    {
                        Data = data,
                        From = msg.From,
                        Message = new Message
                        {
                            Chat = msg.Chat,
                        }
                    };

                    Console.Error.WriteLine($"\nId: {callbackQuery.Message.MessageId}, conversacion Asunt: {conv.Asunto}\n");
                    await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);
                    if (catId == -1)
                    {
                        await renderer.RenderizarProducto(bot, prodId, catId, page, callbackQuery, int.Parse(conv.Asunto!), cantidad);
                        return true;
                    }
                    else
                    {
                        await renderer.RenderizarProducto(bot, prodId, catId, page, callbackQuery, int.Parse(conv.Asunto!), cantidad);
                        return true;
                    }
                }
                else
                {
                    //await bot.SendMessage(msg.Chat.Id, "Valor invalido. Por favor, solo numeros mayores a 0.");
                    await bot.SendPhoto(msg.Chat.Id, url, "Valor invalido. Por favor, solo numeros mayores a 0.", parseMode: ParseMode.Markdown);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handles checkout workflow messages, processing delivery address, location references, and contact phone number.
        /// Manages the multi-step checkout process by tracking the current state through system messages.
        /// </summary>
        /// <param name="bot">The Telegram bot client.</param>
        /// <param name="msg">The message from the user containing checkout information.</param>
        /// <param name="text">The text content of the message (address, references, or phone).</param>
        /// <returns>True if the message was successfully processed as part of the checkout flow, otherwise false.</returns>
        public async Task<bool> ManejoMsgCheckout(ITelegramBotClient bot, Message msg, string text)
        {
            var conv = await _persistencia.ObtenerConversacionActiva(msg.From!.Id);
            if (conv == null) return true;
            var lastMsg = await _persistencia.ObtenerUltimoMensaje(conv.Id);

            if (lastMsg != null && lastMsg.Remitente == TipoRemitente.Sistema)
            {
                string instruction = "";
                string saveMsg = "";
                string Asunto = lastMsg.Contenido.Split('_')[2].Trim('[', ']');
                if (lastMsg.Contenido.Contains("[ESTADO:CHECKOUT_DIRECCION]"))
                {
                    string direccion = text;

                    await _persistencia.ActualizarPedido(msg.From.Id, new PedidoDTO
                    {
                        Estado = EstadoPedido.Pendiente,
                        Direccion = direccion
                    });

                    Asunto = conv.Asunto!;
                    instruction = "📍 PASO 2 :REFERENCIAS DE UBICACION\n\nPor favor, escribe referencias como: \n'frente a la tienda X' o 'casa color verde'";
                    saveMsg = $"[ESTADO:CHECKOUT_REFERENCIAS]_[{Asunto}]_*Esperando referencias...";
                    //await bot.EditMessageText(msg.Chat.Id, int.Parse(conv.Asunto!), instruction, parseMode: ParseMode.Markdown);                    
                }
                else if (lastMsg.Contenido.Contains("[ESTADO:CHECKOUT_REFERENCIAS]"))
                {
                    string referencias = text;

                    await _persistencia.ActualizarPedido(msg.From.Id, new PedidoDTO
                    {
                        Detalles = new PedidoDetalleDTO { Referencias = referencias }
                    });

                    instruction = "📞 PASO 3: TELÉFONO DE CONTACTO\n\nEscribe tu número de teléfono para coordinar la entrega:";
                    saveMsg = $"[ESTADO:CHECKOUT_TELEFONO]_[{Asunto}]_Esperando teléfono...";
                    //await bot.EditMessageText(msg.Chat.Id, int.Parse(Asunto!), instruction, parseMode: ParseMode.Markdown);
                }
                else if (lastMsg.Contenido.Contains("[ESTADO:CHECKOUT_TELEFONO]"))
                {
                    string telefono = text;

                    await _persistencia.ActualizarPedido(msg.From.Id, new PedidoDTO
                    {
                        Detalles = new PedidoDetalleDTO { Telefono = telefono }
                    });

                    CallbackQuery callbackQuery = new()
                    {
                        Data = "menu",
                        From = msg.From,
                        Message = new Message
                        {
                            Chat = msg.Chat,
                        }
                    };
                    saveMsg = "[ESTADO:REPOSO]";
                    await renderer.RenderizarResumenFina(bot, callbackQuery, int.Parse(Asunto!));
                }

                await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);
                if (!string.IsNullOrEmpty(instruction))
                {
                    await bot.EditMessageCaption(msg.Chat.Id, int.Parse(Asunto!), instruction, parseMode: ParseMode.Markdown);
                }
                await _persistencia.RegistrarMensaje(conv.Id, saveMsg, TipoRemitente.Sistema);
                return true;
            }
            return false;
        }

    }
}