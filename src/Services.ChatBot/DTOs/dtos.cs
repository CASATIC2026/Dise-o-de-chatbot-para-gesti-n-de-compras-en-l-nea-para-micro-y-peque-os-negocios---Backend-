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
}