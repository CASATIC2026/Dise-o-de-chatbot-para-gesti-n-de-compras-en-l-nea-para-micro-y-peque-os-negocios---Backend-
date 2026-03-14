namespace Shared.Core.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public ICollection<Conversacion> Conversaciones { get; set; } = new List<Conversacion>();
    }
}