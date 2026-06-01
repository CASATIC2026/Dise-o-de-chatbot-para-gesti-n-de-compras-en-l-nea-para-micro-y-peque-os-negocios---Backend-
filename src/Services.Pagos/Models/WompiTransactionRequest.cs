using System.Text.Json.Serialization;

namespace Services.Pagos.Models;

/// <summary>
/// Represents a request to create a new transaction or payment link via the Wompi payment gateway.
/// </summary>
public class WompiTransactionRequest
{
    /// <summary>
    /// Gets or sets the total amount to be charged in the transaction.
    /// </summary>
    public decimal Monto { get; set; }

    /// <summary>
    /// Gets or sets the unique transaction reference used to track the payment within the system.
    /// </summary>
    public string Referencia { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL where the user should be redirected after completing or canceling the payment process on the gateway.
    /// </summary>
    public string RedirectUrl { get; set; } = "";
}
