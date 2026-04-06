using Services.ChatBot.DTOs;
using Services.ChatBot.Interfaces;
using Services.ChatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Webhook.Controllers.Services;

public class BotRenderer(ITelegramBotClient bot,
IHttpClientFactory httpClientFactory,
ICatalogoUI catalogoUI, IMenuUI menuUI,
ICarrito carritoUI,
IBotPersistencia _persistencia)
{
    private readonly HttpClient _gateway = httpClientFactory.CreateClient("GatewayApi");

    public async Task RenderizarCatalogo(ITelegramBotClient bot, CallbackQuery callbackQuerry, int catId, int page)
    {
        Console.WriteLine($"CatId {callbackQuerry.Data}, Page {page}");
        var data = await _gateway.GetFromJsonAsync<PagedResult<ProductoDTO>>($"productos/list-4/{catId}?page={page}&pageSize=4");
        var categoria = await _gateway.GetFromJsonAsync<CategoriaDTO>($"categorias/{catId}");
        var markup = catalogoUI.BuildUIProductos(data, catId, page);
        if (data == null || !data.Items.Any())
        {
            await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, $" {categoria.Nombre}\n No se encontraron Productos:", replyMarkup: markup);
        }
        else
        {
            // Usamos la interfaz de productos                
            await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, $" {categoria.Nombre}\n 🛍 Productos:", replyMarkup: markup);
        }
    }

    public async Task RenderizarMenu(ITelegramBotClient bot, Message msg, CallbackQuery callbackQuery)
    {
        var telegramId = msg.From!.Id;

        var name = $"{msg.From.FirstName} {msg.From.LastName}".Trim();
        await _persistencia.RegistrarCliente(telegramId, name);

        string tiendaName = "tienda";
        string welcomeText = $"👋 Hola {name}, bienvenido a nuestra {tiendaName}. ";
        Message send;
        var markup = menuUI.BuildUIHome(name);

        if (callbackQuery.Message!.MessageId == 0)
        {
            Console.WriteLine("\nNuevo mensaje para mostrar menu\n");
            send = await bot.SendMessage(msg.Chat, welcomeText, parseMode: ParseMode.Markdown, replyMarkup: markup);
        }
        else
        {
            Console.WriteLine("\nEditando mensaje para mostrar menu\n");
            send = await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId,
                welcomeText, parseMode: ParseMode.Markdown, replyMarkup: markup);
        }
        await _persistencia.ActualizarConversacion(msg.From.Id, send.Id, true);
    }

    public async Task RenderizarCategorias(ITelegramBotClient bot, int page, CallbackQuery callbackQuery)
    {
        var data = await _gateway.GetFromJsonAsync<PagedResult<CategoriaDTO>>($"categorias/list-6?page={page}&pageSize=6");
        if (data == null || !data.Items.Any()) return;
        // Usamos la interfaz de categorías
        var markup = catalogoUI.BuildUICategorias(data, page);
        await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, "📂 Menú:", replyMarkup: markup);
    }

    public async Task RenderizarProducto(ITelegramBotClient bot, int prodId, int catId, int page, CallbackQuery callbackQuery,int msgId, int cantidad)
    {
        var data = await _gateway.GetFromJsonAsync<ProductoDTO>($"productos/{prodId}");
        if (data == null) return;

        string msg = $"📦 {data.Nombre}\n" +
        $"\n\tPrecio: ${data.Precio}" +
        $"\n\tStock: {data.StockDisponible}";

        var keyboard = catalogoUI.BuildUIDetalleProducto(prodId, catId, page, cantidad);

        await bot.EditMessageText(callbackQuery.Message!.Chat, msgId, msg, replyMarkup: keyboard);

        Console.WriteLine($"name {data.Nombre}, precio {data.Precio}, stock {data.StockDisponible}");
    }

    public async Task RenderizarCarrito(ITelegramBotClient bot, CallbackQuery callbackQuery, int msgId)
    {
        var pedido = await _persistencia.ObtenerPedidoActivo(callbackQuery.From.Id);
        var (texto, markup) = carritoUI.BuildUICarrito(pedido);

        await bot.EditMessageText(callbackQuery.Message!.Chat, msgId, texto, replyMarkup: markup);

        await bot.AnswerCallbackQuery(callbackQuery.Id);
    }
}