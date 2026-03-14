using System;

namespace Shared.Core.Entities
{
    // Enum del estado del pago
    public enum EstadoPago
    {
        Pendiente = 1,
        Completado = 2,
        Rechazado = 3,
        Cancelado = 4
    }

    public class Pago
    {
        public int Id { get; set; }

        // Relación con Pedido
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        public decimal Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        // Usamos el enum en lugar de string
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        public string? ReferenciaTransaccion { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
    }
}