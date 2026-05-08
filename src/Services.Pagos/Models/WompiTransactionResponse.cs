using System.Text.Json.Serialization;

namespace Services.Pagos.Models;

/// <summary>
/// Represents the internal response structure for a Wompi transaction or payment link generation request.
/// </summary>
public class WompiTransactionResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the unique transaction identifier assigned by the payment gateway.
    /// </summary>
    public string TransactionId { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL for the generated payment link.
    /// </summary>
    public string PaymentLink { get; set; } = "";

    /// <summary>
    /// Gets or sets the current status of the transaction.
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// Gets or sets the error message if the operation failed.
    /// </summary>
    public string? Error { get; set; }
}