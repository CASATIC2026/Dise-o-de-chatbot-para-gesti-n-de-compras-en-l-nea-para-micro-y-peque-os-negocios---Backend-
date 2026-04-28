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
        public int Id { get; set; } = 0;
        public string Nombre { get; set; } = "";
    }

    /// <summary>
    /// Data Transfer Object representing product information.
    /// </summary>
    public class ProductoDTO
    {
        public int Id { get; set; } = 0;
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public int StockDisponible { get; set; }
        public string ImagenUrl { get; set; } = "";
    }

    /// <summary>
    /// Data Transfer Object for client/user information managed by the bot.
    /// </summary>
    public class ClienteDTO
    {
        public long TelegramId { set; get; }
        public string? Nombre { set; get; }
        public string? Direccion { set; get; }
        public string? Telefono { set; get; }
        public string? Email { set; get; }
    }

    /// <summary>
    /// Data Transfer Object representing an order's status and summary.
    /// </summary>
    public class PedidoDTO
    {
        public int Id { get; set; } = 0;
        public DateTime FechaRealizado { get; set; }
        public EstadoPedido Estado { get; set; }
        public decimal? Total { get; set; }
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

    public class PagosLinksDTO
    {
        public string Url {get; set;} = string.Empty;
        public string Referencia {get; set;} = string.Empty;
        public int EstadoPedido {get; set;} = 0;
        public int EstadoPago {get; set;} = 0;
    }

    public class NotificacionPagosDTO
    {
        public string Referencia {get; set;} = string.Empty;
        public int EstadoPago {get; set;} = 0;
    }    
}