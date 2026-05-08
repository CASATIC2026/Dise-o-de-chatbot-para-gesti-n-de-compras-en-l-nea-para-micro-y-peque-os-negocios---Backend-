using System;

namespace Shared.Core.Entities
{
    /// <summary>
    /// Defines the possible states for a payment transaction within the system.
    /// </summary>
    public enum EstadoPago
    {
        /// <summary>The payment has been initiated but is awaiting confirmation from the gateway.</summary>
        Pendiente = 1,
        /// <summary>The payment has been successfully processed and confirmed.</summary>
        Completado = 2,
        /// <summary>The payment was rejected by the gateway or failed during processing.</summary>
        Rechazado = 3,
        /// <summary>The payment was cancelled by the user or the system before completion.</summary>
        Cancelado = 4
    }

    /// <summary>
    /// Represents a payment record associated with a specific order.
    /// </summary>
    public class Pago
    {
        /// <summary>
        /// Gets or sets the unique identifier for the payment.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the order associated with this payment.
        /// </summary>
        public int PedidoId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the associated order.
        /// </summary>
        public virtual Pedido? Pedido { get; set; }

        /// <summary>
        /// Gets or sets the total amount processed in this payment.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Gets or sets the name of the payment method used (e.g., "WOMPI").
        /// </summary>
        public string MetodoPago { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current status of the payment.
        /// </summary>
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        /// <summary>
        /// Gets or sets the external transaction reference provided by the payment gateway.
        /// </summary>
        public string? ReferenciaTransaccion { get; set; }

        /// <summary>
        /// Gets or sets the specific date and time when the payment was finalized.
        /// </summary>
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the payment record was created in the system.
        /// </summary>
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the payment record was last modified.
        /// </summary>
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
    }
}