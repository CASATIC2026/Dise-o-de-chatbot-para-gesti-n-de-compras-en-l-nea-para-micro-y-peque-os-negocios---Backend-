using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Services.Pagos.Models;

namespace Services.Pagos.Services;

/// <summary>
/// Service responsible for interacting with the Wompi payment gateway API.
/// Handles authentication, payment link creation, and link management.
/// </summary>
public class WompiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WompiService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WompiService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for API requests.</param>
    /// <param name="configuration">The configuration provider for accessing Wompi settings.</param>
    /// <param name="logger">The logger instance for diagnostic messages.</param>
    public WompiService(HttpClient httpClient, IConfiguration configuration, ILogger<WompiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Obtains an OAuth2 access token from the Wompi identity service using client credentials.
    /// </summary>
    /// <returns>A JWT access token string if successful; otherwise, an empty string.</returns>
    private async Task<string> ObtenerTokenAsync()
    {
        var clientId = _configuration["Wompi:ClientId"] ?? _configuration["WOMPI_CLIENT_ID"];
        var clientSecret = _configuration["Wompi:ClientSecret"] ?? _configuration["WOMPI_CLIENT_SECRET"];

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId ?? string.Empty),
            new KeyValuePair<string, string>("client_secret", clientSecret ?? string.Empty),
            new KeyValuePair<string, string>("audience", "wompi_api")
        });

        var response = await _httpClient.PostAsync("https://id.wompi.sv/connect/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error obteniendo token de Wompi: {Error}", responseContent);
            return string.Empty;
        }

        var authResponse = JsonSerializer.Deserialize<WompiAuthResponse>(responseContent);
        if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
        {
            _logger.LogError("Token de Wompi vacio o invalido.");
            return string.Empty;
        }

        _logger.LogInformation("Token de Wompi obtenido correctamente.");
        return authResponse.AccessToken;
    }

    /// <summary>
    /// Creates a payment link in Wompi for a specific transaction.
    /// Configures redirection URLs, webhooks, and usage limits based on the request and system settings.
    /// </summary>
    /// <param name="request">The transaction request details including amount and reference.</param>
    /// <returns>A <see cref="WompiTransactionResponse"/> containing the payment link or error details.</returns>
    /// <exception cref="Exception">Thrown when an authentication token cannot be acquired.</exception>
    public async Task<WompiTransactionResponse> CrearEnlacePago(WompiTransactionRequest request)
    {
        try
        {
            var token = await ObtenerTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("No se pudo obtener el token de Wompi.");
            }

            var notificationEmails = _configuration["Wompi:NotificationEmails"];
            var webhookUrl = ResolveWebhookUrl();
            var maxSuccessfulPayments = Math.Max(1, _configuration.GetValue<int?>("Wompi:MaxSuccessfulPayments") ?? 1);
            Dictionary<string, object?>? configuracion = null;

            if (!string.IsNullOrWhiteSpace(request.RedirectUrl))
            {
                configuracion ??= new Dictionary<string, object?>();
                configuracion["urlRedirect"] = request.RedirectUrl;
            }

            if (!string.IsNullOrWhiteSpace(webhookUrl))
            {
                configuracion ??= new Dictionary<string, object?>();
                configuracion["urlWebhook"] = webhookUrl;
            }

            if (!string.IsNullOrWhiteSpace(notificationEmails))
            {
                configuracion ??= new Dictionary<string, object?>();
                configuracion["notificarTransaccionCliente"] = true;
                configuracion["emailsNotificacion"] = notificationEmails;
            }

            if (configuracion != null)
            {
                configuracion["esRecurrente"] = false;
                configuracion["esMontoEditable"] = false;
                configuracion["esCantidadEditable"] = false;
                configuracion["cantidadPorDefecto"] = 1;
            }

            var payload = new Dictionary<string, object?>
            {
                ["identificadorEnlaceComercio"] = request.Referencia,
                ["monto"] = request.Monto,
                ["nombreProducto"] = "Pedido " + request.Referencia,
                ["formaPago"] = new Dictionary<string, object?>
                {
                    ["permitirTarjetaCreditoDebido"] = true,
                    ["permitirPagoConPuntoAgricola"] = false,
                    ["permitirPagoEnCuotasAgricola"] = false,
                    ["permitirPagoEnBitcoin"] = false,
                    ["permitePagoQuickPay"] = false
                },
                ["limitesDeUso"] = new Dictionary<string, object?>
                {
                    ["cantidadMaximaPagosExitosos"] = maxSuccessfulPayments,
                    ["cantidadMaximaPagosFallidos"] = 1
                }
            };

            _logger.LogInformation(
                "Creando enlace Wompi para referencia {Referencia} con maximo {MaxPagos} pago(s) exitoso(s).",
                request.Referencia,
                maxSuccessfulPayments);

            if (configuracion != null)
            {
                payload["configuracion"] = configuracion;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsJsonAsync("https://api.wompi.sv/EnlacePago", payload);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error Wompi SV: {Response}", responseContent);
                
                if (responseContent.Contains("identificadorEnlaceComercio_duplicado"))
                {
                    return new WompiTransactionResponse { Success = false, Error = "La referencia de este pedido ya fue utilizada en otro enlace de pago." };
                }

                return new WompiTransactionResponse
                {
                    Success = false,
                    Error = responseContent
                };
            }

            var result = JsonSerializer.Deserialize<WompiEnlaceResponse>(responseContent);
            if (result == null)
            {
                return new WompiTransactionResponse
                {
                    Success = false,
                    Error = "Respuesta invalida de Wompi"
                };
            }

            _logger.LogInformation("Enlace de pago creado correctamente: {Url}", result.UrlEnlace);
            return new WompiTransactionResponse
            {
                Success = true,
                TransactionId = result.IdEnlace.ToString(),
                PaymentLink = result.UrlEnlace,
                Status = "ACTIVO"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando enlace Wompi SV");
            return new WompiTransactionResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Requests Wompi to deactivate an existing payment link.
    /// </summary>
    /// <param name="enlaceId">The unique identifier of the payment link in Wompi.</param>
    /// <returns>True if the link was successfully deactivated; otherwise, false.</returns>
    public async Task<bool> DesactivarEnlacePago(long enlaceId)
    {
        try
        {
            var token = await ObtenerTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("No se pudo obtener token para desactivar el enlace {EnlaceId}.", enlaceId);
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PutAsync($"https://api.wompi.sv/EnlacePago/{enlaceId}/desactivar", content: null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "No se pudo desactivar el enlace Wompi {EnlaceId}. Status: {StatusCode}. Body: {Body}",
                    enlaceId,
                    response.StatusCode,
                    responseContent);
                return false;
            }

            _logger.LogInformation("Enlace Wompi {EnlaceId} desactivado correctamente.", enlaceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error desactivando enlace Wompi {EnlaceId}", enlaceId);
            return false;
        }
    }

    /// <summary>
    /// Resolves the correct Webhook URL from configuration, supporting both modern and legacy keys.
    /// </summary>
    /// <returns>The resolved absolute URL string, or null if no valid configuration is found.</returns>
    private string? ResolveWebhookUrl()
    {
        var webhookUrl = _configuration["Wompi:WebhookUrl"];
        if (!string.IsNullOrWhiteSpace(webhookUrl))
        {
            return webhookUrl;
        }

        var legacyValue = _configuration["Wompi:WebhookSecret"];
        if (Uri.TryCreate(legacyValue, UriKind.Absolute, out var legacyUri)
            && (legacyUri.Scheme == Uri.UriSchemeHttp || legacyUri.Scheme == Uri.UriSchemeHttps))
        {
            _logger.LogWarning("Usando Wompi:WebhookSecret como fallback de URL de webhook. Conviene migrar a Wompi:WebhookUrl.");
            return legacyValue;
        }

        _logger.LogWarning("No se encontro una URL de webhook valida para Wompi. Los cambios de estado no se reflejaran automaticamente.");
        return null;
    }
}
