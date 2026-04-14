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

    public async Task RenderizarProducto(ITelegramBotClient bot, int prodId, int catId, int page, CallbackQuery callbackQuery, int msgId, int cantidad)
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

    public async Task RenderizarResumenFina(ITelegramBotClient bot, CallbackQuery callbackQuery, int msgId)
    {
        Console.WriteLine($"\nClienteId Telegram: {callbackQuery.From.Id}\n");
        var pedido = await _persistencia.ObtenerPedidoActivo(callbackQuery.From.Id);
        var cliente = await _persistencia.ObtenerCliente(callbackQuery.From.Id);

        // 2. Generamos la UI de confirmación
        var (texto, markup) = carritoUI.BuildUIResumenFinal(pedido, cliente);

        // 3. Editamos el mensaje original usando el Asunto transportado
        await bot.EditMessageText(callbackQuery.Message!.Chat.Id, msgId, texto,
            parseMode: ParseMode.Markdown, replyMarkup: markup);
    }
    public async Task RenderizarOrdenes(ITelegramBotClient bot, CallbackQuery callbackQuery, int page)
    {
        var (pedidos, count) = await _persistencia.ObtenerPedidosUsuario(callbackQuery.From.Id, 6, page);
        PagedResult<PedidoDTO> data = new();
        if (pedidos != null)
        {
            Console.WriteLine("Ordenes: " + count);
            data = new()
            {
                Items = pedidos.Select(p => new PedidoDTO
                {
                    Id = p.Id,
                    FechaRealizado = p.CreadoEn,
                    Estado = p.Estado,
                    Total = Decimal.Parse(p.Total.ToString()),
                }).ToList(),
                TotalCount = count
            };
        }
        else
        {
            data = new();
        }

        var (markup, texto) = catalogoUI.BuildUIPedidos(data, page);

        await bot.EditMessageText(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, texto, parseMode: ParseMode.Markdown, replyMarkup: markup);
    }
}