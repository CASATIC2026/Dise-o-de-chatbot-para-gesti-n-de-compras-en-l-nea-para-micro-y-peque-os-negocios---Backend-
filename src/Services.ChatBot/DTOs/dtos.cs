using Shared.Core.Entities;

namespace Services.ChatBot.DTOs
{
    /// <summary>
    /// Represents a paginated set of items of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the list of items for the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();
        /// <summary>
        /// Gets or sets the total number of items available across all pages.
        /// </summary>
        public int TotalCount { get; set; } = 0;
    }

    /// <summary>
    /// Data Transfer Object for product categories.
    /// </summary>
    public class CategoriaDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the category.
        /// </summary>
        public int Id { get; set; } = 0;
        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Nombre { get; set; } = "";
    }

    /// <summary>
    /// Data Transfer Object representing product information.
    /// </summary>
    public class ProductoDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// </summary>
        public int Id { get; set; } = 0;
        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        public string Nombre { get; set; } = "";
        /// <summary>
        /// Gets or sets the detailed description of the product.
        /// </summary>
        public string Descripcion { get; set; } = "";
        /// <summary>
        /// Gets or sets the unit price of the product.
        /// </summary>
        public decimal Precio { get; set; }
        /// <summary>
        /// Gets or sets the current available stock for the product.
        /// </summary>
        public int StockDisponible { get; set; }
        /// <summary>
        /// Gets or sets the URL for the product's display image.
        /// </summary>
        public string ImagenUrl { get; set; } = "";
    }

    /// <summary>
    /// Data Transfer Object for client/user information managed by the bot.
    /// </summary>
    public class ClienteDTO
    {
        /// <summary>
        /// Gets or sets the unique Telegram ID of the client.
        /// </summary>
        public long TelegramId { set; get; }
        /// <summary>
        /// Gets or sets the full name of the client.
        /// </summary>
        public string? Nombre { set; get; }
        /// <summary>
        /// Gets or sets the physical delivery address of the client.
        /// </summary>
        public string? Direccion { set; get; }
        /// <summary>
        /// Gets or sets the contact phone number of the client.
        /// </summary>
        public string? Telefono { set; get; }
        /// <summary>
        /// Gets or sets the email address of the client.
        /// </summary>
        public string? Email { set; get; }
    }

    /// <summary>
    /// Data Transfer Object representing an order's status and summary.
    /// </summary>
    public class PedidoDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the order.
        /// </summary>
        public int Id { get; set; } = 0;
        /// <summary>
        /// Gets or sets the date and time when the order was placed.
        /// </summary>
        public DateTime FechaRealizado { get; set; }
        /// <summary>
        /// Gets or sets the current status of the order.
        /// </summary>
        public EstadoPedido Estado { get; set; }
        /// <summary>
        /// Gets or sets the total monetary amount for the order.
        /// </summary>
        public decimal? Total { get; set; }
        /// <summary>
        /// Gets or sets the delivery address for this specific order.
        /// </summary>
        public string? Direccion { get; set; }
        /// <summary>
        /// Gets or sets additional contact and reference details for the order.
        /// </summary>
        public PedidoDetalleDTO? Detalles { get; set; } = null;
    }

    /// <summary>
    /// Data Transfer Object for extra order details like contact info and landmarks.
    /// </summary>
    public class PedidoDetalleDTO
    {
        /// <summary>
        /// Gets or sets location references or landmarks for delivery.
        /// </summary>
        public string? Referencias { get; set; }
        /// <summary>
        /// Gets or sets the contact phone number for this specific order.
        /// </summary>
        public string? Telefono { get; set; }
        /// <summary>
        /// Gets or sets the contact email for this specific order.
        /// </summary>
        public string? Email { get; set; }
    }

    /// <summary>
    /// Data Transfer Object containing links and references for payment processing.
    /// </summary>
    public class PagosLinksDTO
    {
        /// <summary>
        /// Gets or sets the URL to the external payment gateway.
        /// </summary>
        public string Url {get; set;} = string.Empty;
        /// <summary>
        /// Gets or sets the unique transaction reference.
        /// </summary>
        public string Referencia {get; set;} = string.Empty;
        /// <summary>
        /// Gets or sets the numerical representation of the order status.
        /// </summary>
        public int EstadoPedido {get; set;} = 0;
        /// <summary>
        /// Gets or sets the numerical representation of the payment status.
        /// </summary>
        public int EstadoPago {get; set;} = 0;
    }

    /// <summary>
    /// Data Transfer Object for incoming payment status notifications.
    /// </summary>
    public class NotificacionPagosDTO
    {
        /// <summary>
        /// Gets or sets the transaction reference linked to the notification.
        /// </summary>
        public string Referencia {get; set;} = string.Empty;
        /// <summary>
        /// Gets or sets the updated numerical status of the payment.
        /// </summary>
        public int EstadoPago {get; set;} = 0;
        /// <summary>
        /// Gets or sets the redirection URL associated with the payment status.
        /// </summary>
        public string Url {get; set;} = string.Empty;
    
    }    
}