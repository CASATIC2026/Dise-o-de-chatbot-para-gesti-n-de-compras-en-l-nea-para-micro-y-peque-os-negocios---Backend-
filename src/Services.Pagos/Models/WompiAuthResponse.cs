using System.Text.Json.Serialization;

namespace Services.Pagos.Models;

/// <summary>
/// Represents the authentication response received from the Wompi identity service.
/// </summary>
public class WompiAuthResponse
{
    /// <summary>
    /// Gets or sets the access token used to authorize subsequent API requests.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    /// <summary>
    /// Gets or sets the lifetime of the access token in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the type of token issued (e.g., "Bearer").
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "";
}