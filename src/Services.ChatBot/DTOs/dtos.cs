using Shared.Core.Entities;

namespace Services.ChatBot.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; } = 0;
    }

    public class CategoriaDTO
    {
        public int Id { get; set; } = 0;
        public string Nombre { get; set; } = "";
    }

    public class ProductoDTO
    {
        public int Id { get; set; } = 0;
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int StockDisponible { get; set; }
    }

    public class ClienteDTO
    {
        public long TelegramId { set; get; }
        public string? Nombre { set; get; }
        public string? Direccion { set; get; }
        public string? Telefono { set; get; }
        public string? Email { set; get; }
    }

    public class PedidoDTO
    {
        public int Id { get; set; } = 0;
        public DateTime FechaRealizado { get; set; }
        public EstadoPedido Estado { get; set; }
        public decimal? Total { get; set; }
        public string? Direccion { get; set; }        
        public PedidoDetalleDTO? Detalles { get; set; } = null;
    }

    public class PedidoDetalleDTO
    {
        public string? Referencias { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }
}