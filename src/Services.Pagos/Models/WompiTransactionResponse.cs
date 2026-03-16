using System.Text.Json.Serialization;

namespace Services.Pagos.Models;

public class WompiTransactionResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = "";
    public string PaymentLink { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Error { get; set; }
}