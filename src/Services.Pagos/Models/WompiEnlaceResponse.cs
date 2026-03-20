using System.Text.Json.Serialization;

namespace Services.Pagos.Models;

public class WompiEnlaceResponse
{
    [JsonPropertyName("idEnlace")]
    public long IdEnlace { get; set; }

    [JsonPropertyName("urlEnlace")]
    public string UrlEnlace { get; set; } = "";
}