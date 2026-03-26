using System.Text.Json.Serialization;

namespace Services.Pagos.Models;

public class WompiTransactionRequest
{
    public decimal Monto { get; set; }
    public string Referencia { get; set; } = "";
    public string RedirectUrl { get; set; } = "";
}

