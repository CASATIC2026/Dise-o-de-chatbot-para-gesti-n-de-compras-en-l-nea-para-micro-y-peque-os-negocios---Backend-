using Services.ChatBot.DTOs;
using Services.ChatBot.Interfaces;
using Services.ChatBot.Utils;
using Shared.Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Webhook.Controllers.Services;

/// <summary>
/// Service responsible for rendering various UI components and messages to the Telegram bot.
/// It coordinates between UI builders, the API gateway, and persistence layers to provide a visual interface.
/// </summary>
/// <param name="httpClientFactory">The factory used to create HTTP clients for API communication.</param>
/// <param name="catalogoUI">Interface for building catalog-related UI components.</param>
/// <param name="menuUI">Interface for building menu UI components.</param>
/// <param name="carritoUI">Interface for building shopping cart UI components.</param>
/// <param name="_persistencia">Interface for handling bot-related data persistence.</param>
public class BotRenderer(IHttpClientFactory httpClientFactory,
ICatalogoUI catalogoUI, IMenuUI menuUI,
ICarrito carritoUI,
IBotPersistencia _persistencia
)
{
    /// <summary>The HTTP client used to interact with the system gateway.</summary>
    private readonly HttpClient _gateway = httpClientFactory.CreateClient("GatewayApi");
    private readonly string url = "https://placehold.co/360x100/png?text=Tienda";

    /// <summary>
    /// Renders the product catalog for a specific category with pagination.
    /// Displays products as an interactive list with buttons for selection and navigation.
    /// </summary>
    /// <param name="bot">The Telegram bot client.</param>
    /// <param name="callbackQuerry">The callback query from the user's interaction.</param>
    /// <param name="catId">The category ID to display products for.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RenderizarCatalogo(ITelegramBotClient bot, CallbackQuery callbackQuerry, int catId, int page)
    {
        //Console.WriteLine($"CatId {callbackQuerry.Data}, Page {page}");
        try
        {
            var data = await _gateway.GetFromJsonAsync<PagedResult<ProductoDTO>>($"productos/list-4/{catId}?page={page}&pageSize=4");

            var categoria = await _gateway.GetFromJsonAsync<CategoriaDTO>($"categorias/{catId}");
            var markup = catalogoUI.BuildUIProductos(data, catId, page);

            var caption = data == null || !data.Items.Any()
                ? $" {categoria.Nombre}\n No se encontraron Productos:"
                : $" {categoria.Nombre}\n 🛍 Productos:";

            var media = new InputMediaPhoto(url)
            {
                Caption = caption,
                ParseMode = ParseMode.Markdown
            };

            //await bot.EditMessageText(callbackQuerry.Message!.Chat, callbackQuerry.Message.MessageId, caption, replyMarkup: markup);
            await bot.EditMessageMedia(callbackQuerry.Message!.Chat.Id, callbackQuerry.Message.MessageId, media, replyMarkup: markup);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fallo al renderizar catalogo: " + ex);
        }
    }

    /// <summary>
    /// Renders the main menu displaying the store welcome message and navigation options.
    /// Registers the client and creates or updates the active conversation.
    /// </summary>
    /// <param name="bot">The Telegram bot client.</param>
    /// <param name="msg">The message object containing chat information.</param>
    /// <param name="callbackQuery">The callback query containing user information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RenderizarMenu(ITelegramBotClient bot, Message msg, CallbackQuery callbackQuery)
    {
        var user = callbackQuery?.From ?? msg?.From;
        if (user == null) return;
        var telegramId = user.Id;

        var name = $"{user.FirstName} {user.LastName}".Trim();
        await _persistencia.RegistrarCliente(telegramId, name);

        string tiendaName = "tienda";
        string welcomeText = $"👋 Hola {name}, bienvenido a nuestra {tiendaName}. ";

        //Message send;
        var markup = menuUI.BuildUIHome(name);

        if (callbackQuery.Message!.MessageId == 0)
        {
            Console.WriteLine("\nNuevo mensaje para mostrar menu\n");

            var send = await bot.SendPhoto(msg.Chat, url, welcomeText, parseMode: ParseMode.Markdown, replyMarkup: markup);
            //var send = await bot.SendMessage(msg.Chat, welcomeText, parseMode: ParseMode.Markdown, replyMarkup: markup);
            await _persistencia.ActualizarConversacion(telegramId, send.Id, true);
        }
        else
        {
            Console.WriteLine("\nEditando mensaje para mostrar menu\n");
            //await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId,
            //    welcomeText, parseMode: ParseMode.Markdown, replyMarkup: markup);
            await bot.EditMessageCaption(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId,
                welcomeText, parseMode: ParseMode.Markdown, replyMarkup: markup);
            await _persistencia.ActualizarConversacion(telegramId, callbackQuery.Message.MessageId, true);
        }
    }

    /// <summary>
    /// Renders the category selection menu with pagination.
    /// Displays available product categories as interactive buttons.
    /// </summary>
    /// <param name="bot">The Telegram bot client.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="callbackQuery">The callback query from the user's interaction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RenderizarCategorias(ITelegramBotClient bot, int page, CallbackQuery callbackQuery)
    {
        var data = await _gateway.GetFromJsonAsync<PagedResult<CategoriaDTO>>($"categorias/list-6?page={page}&pageSize=6");
        if (data == null || !data.Items.Any()) return;
        // Usamos la interfaz de categorías
        var markup = catalogoUI.BuildUICategorias(data, page);
        //await bot.EditMessageText(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, "📂 Menú:", replyMarkup: markup);
        await bot.EditMessageCaption(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, "📂 Menú:", replyMarkup: markup);
    }

    /// <summary>
    /// Renders a single product detail view with price, stock information, and quantity controls.
    /// Displays the product image and interactive buttons for quantity adjustment and adding to cart.
    /// </summary>
    /// <param name="bot">The Telegram bot client.</param>
    /// <param name="prodId">The product ID to display.</param>
    /// <param name="catId">The category ID the product belongs to.</param>
    /// <param name="page">The page number from the catalog for navigation.</param>
    /// <param name="callbackQuery">The callback query from the user's interaction.</param>
    /// <param name="msgId">The message ID to edit.</param>
    /// <param name="cantidad">The initial quantity selected for the product.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RenderizarProducto(ITelegramBotClient bot, int prodId, int catId, int page, CallbackQuery callbackQuery, int msgId, int cantidad)
    {
        var data = await _gateway.GetFromJsonAsync<ProductoDTO>($"productos/{prodId}");
        if (data == null) return;
        string productoImg = data.ImagenUrl;
        string msg = $"📦 {data.Nombre}\n" +
        $"\n\tPrecio: ${data.Precio}" +
        $"\n\tStock: {data.StockDisponible}";

        var keyboard = catalogoUI.BuildUIDetalleProducto(prodId, catId, page, cantidad);

        var media = new InputMediaPhoto(productoImg)
        {
            Caption = msg,
            ParseMode = ParseMode.Markdown
        };

        //await bot.EditMessageText(callbackQuery.Message!.Chat, msgId, msg, replyMarkup: keyboard);
        await bot.EditMessageMedia(callbackQuery.Message!.Chat.Id, msgId, media, replyMarkup: keyboard);

        Console.WriteLine($"name {data.Nombre}, precio {data.Precio}, stock {data.StockDisponible}");
    }

    /// <summary>
    /// Renders the shopping cart display showing all items, quantities, prices, and cart management options.
    /// </summary>
    /// <param name="bot">The Telegram bot client.</param>
    /// <param name="callbackQuery">The callback query from the user's interaction.</param>
    /// <param name="msgId">The message ID to edit.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RenderizarCarrito(ITelegramBotClient bot, CallbackQuery callbackQuery, int msgId)
    {
        var pedido = await _persistencia.ObtenerPedidoActivo(callbackQuery.From.Id);
        var (texto, markup) = carritoUI.BuildUICarrito(pedido);
        string caption = texto ?? "";
        var media = new InputMediaPhoto(url)
        {
            Caption = caption,
            ParseMode = ParseMode.Markdown
        };
        //await bot.EditMessageText(callbackQuery.Message!.Chat, msgId, texto, replyMarkup: markup);
        await bot.EditMessageMedia(callbackQuery.Message!.Chat.Id, msgId, media, replyMarkup: markup);

        await bot.AnswerCallbackQuery(callbackQuery.Id);
    }

    /// <summary>
    /// Renders the final checkout summary with order details, total price, and delivery information.
    /// Displays the complete order confirmation before finalization.
    /// </summary>
    /// <param name="bot">The Telegram bot client.</param>
    /// <param name="callbackQuery">The callback query from the user's interaction.</param>
    /// <param name="msgId">The message ID to edit.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RenderizarResumenFina(ITelegramBotClient bot, CallbackQuery callbackQuery, int msgId)
    {
        Console.WriteLine($"\nClienteId Telegram: {callbackQuery.From.Id}\n");
        var pedido = await _persistencia.ObtenerPedidoActivo(callbackQuery.From.Id);
        var cliente = await _persistencia.ObtenerCliente(callbackQuery.From.Id);

        // 2. Generamos la UI de confirmación
        var (texto, markup) = carritoUI.BuildUIResumenFinal(pedido, cliente);


        // 3. Editamos el mensaje original usando el Asunto transportado
        //await bot.EditMessageText(callbackQuery.Message!.Chat.Id, msgId, texto,
        //    parseMode: ParseMode.Markdown, replyMarkup: markup);
        await bot.EditMessageCaption(callbackQuery.Message!.Chat.Id, msgId, texto,
            parseMode: ParseMode.Markdown, replyMarkup: markup);
    }

    /// <summary>
    /// Renders the user's order history with pagination.
    /// Displays a list of past and current orders with status and total information.
    /// </summary>
    /// <param name="bot">The Telegram bot client.</param>
    /// <param name="callbackQuery">The callback query from the user's interaction.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

        //await bot.EditMessageText(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, texto, parseMode: ParseMode.Markdown, replyMarkup: markup);
        await bot.EditMessageCaption(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, texto, parseMode: ParseMode.Markdown, replyMarkup: markup);
    }

    /// <summary>
    /// Renders a payment ticket (receipt) for the user, containing order details and a redirection link to the payment gateway.
    /// It also attempts to finalize the order status in the persistence layer.
    /// </summary>
    /// <param name="bot">The Telegram bot client instance.</param>
    /// <param name="callbackQuery">The callback query originating from the user's interaction.</param>
    /// <param name="data">The payment link and reference data generated by the payment service.</param>
    /// <param name="pedido">The order information being processed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RenderizarTicket(ITelegramBotClient bot, CallbackQuery callbackQuery, PagosLinksDTO data, PedidoDTO pedido)
    {
        var Succes = false; var msg = "";
        //var data = await _paymentService.GeneratedPaymentLink(pedido.Id);        
        if (pedido == null) return;
        string urlCodec = Uri.EscapeDataString(data.Url);
        string urlPublic = "https://adele-unconvergent-preternaturally.ngrok-free.dev/api/pagos/redirect";
        
        string url = $"{urlPublic}?url={urlCodec}&convasacionId={callbackQuery!.Message!.MessageId}&refe={data.Referencia}"; //cambio de url de servicio por puerto, al subir cambiar por url de microservicio generado

        
        try
        {
            var (texto, markup) = carritoUI.Ticket(data, pedido.Id, url);


            if (string.IsNullOrEmpty(texto))
                (Succes, msg) = await _persistencia.ActualizarPedido(callbackQuery.From.Id, pedido); // En renderer
            else
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, $"⚠️ {texto}", showAlert: true);
                await bot.EditMessageCaption(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, texto, parseMode: ParseMode.Markdown, replyMarkup: markup);
                return;
            }
            if (string.IsNullOrEmpty(texto))
                (Succes, msg) = await _persistencia.ActualizarPedido(callbackQuery.From.Id, pedido); // En renderer
            else
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, $"⚠️ {texto}", showAlert: true);
                await bot.EditMessageCaption(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, texto, parseMode: ParseMode.Markdown, replyMarkup: markup);
                return;
            }

            
            if (Succes)
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, $"⚠️ {msg}", showAlert: true);
                await bot.EditMessageCaption(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, texto, parseMode: ParseMode.Markdown, replyMarkup: markup);
            }
            else
            {
                var text = "🔄 REINTENTAR";
                await bot.AnswerCallbackQuery(callbackQuery.Id, $"⚠️ {msg}", showAlert: true);
                var markup2 = new InlineKeyboardMarkup(new[]{
                InlineKeyboardButton.WithCallbackData(text, "checkoutEnd"),
                InlineKeyboardButton.WithCallbackData("🛒 Volver al Carrito", "cart")
            });
                await bot.EditMessageCaption(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, text, parseMode: ParseMode.Markdown, markup2);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fallo al renderizar el ticket: " + ex);
        }
    }
    public async Task RenderizarDepartamentos(ITelegramBotClient bot, CallbackQuery callbackQuery, int page)
    {
        var JsonUtil = new DeserialiceJson();

        try
        {
            var divisionPolitica = JsonUtil.ObtenerDatos();
            List<String> departamentos = divisionPolitica.Keys.ToList();
            var (texto, markup) = carritoUI.Deptos(departamentos, page);
            await bot.EditMessageCaption(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, texto, parseMode: ParseMode.Markdown, markup);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al renderizar departamentos: " + ex.Message);            
        }
    }

    public async Task RenderizarMunicipios(ITelegramBotClient bot, CallbackQuery callbackQuery, string departamento , int page, int deptoPage)
    {
        var JsonUtil = new DeserialiceJson();
        try
        {
            var divisionPolitica = JsonUtil.ObtenerDatos();
            List<String> municipios = divisionPolitica[departamento].ToList();
            var (texto, markup) = carritoUI.Municipios(municipios, page, deptoPage, departamento);
            await bot.EditMessageCaption(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, texto, parseMode: ParseMode.Markdown, markup);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al renderizar municipios: " + ex.Message);            
        }
    }
}