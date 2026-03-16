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
    }
}