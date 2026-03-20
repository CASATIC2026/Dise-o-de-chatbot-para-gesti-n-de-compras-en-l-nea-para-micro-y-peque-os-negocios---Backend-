using System.Text.Json.Serialization;

namespace Services.Pagos.Models
{
    public class WompiWebhookRequest
    {
        [JsonPropertyName("idTransaccion")]
        public long IdTransaccion { get; set; }

        [JsonPropertyName("referencia")]
        public string Referencia { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("monto")]
        public decimal Monto { get; set; }
    }
}