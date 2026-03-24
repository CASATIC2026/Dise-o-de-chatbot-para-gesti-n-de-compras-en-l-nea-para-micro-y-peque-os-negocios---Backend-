using System.ComponentModel.DataAnnotations;

namespace Shared.Core.Entities;

public class Producto
{

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int StockTotal { get; set; }

    public int? StockReservado {get; set;}

    public int? StockDisponible {get; private set;}
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
    public ICollection<PedidoProducto> PedidoProductos { get; set; } = new List<PedidoProducto>();
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    // Relación N:N → Un producto puede estar en muchos pedidos y un pedido puede tener muchos productos
}
