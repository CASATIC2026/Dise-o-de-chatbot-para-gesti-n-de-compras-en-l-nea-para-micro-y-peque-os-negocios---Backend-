using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Services.Pagos.Models;

namespace Services.Pagos.Services;

public class WompiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WompiService> _logger;

    public WompiService(HttpClient httpClient, IConfiguration configuration, ILogger<WompiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    // 1️⃣ Obtener token OAuth de Wompi
    private async Task<string> ObtenerTokenAsync()
    {
        var clientId = _configuration["Wompi:ClientId"];
        var clientSecret = _configuration["Wompi:ClientSecret"];

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string,string>("grant_type", "client_credentials"),
            new KeyValuePair<string,string>("client_id", clientId ?? ""),
            new KeyValuePair<string,string>("client_secret", clientSecret ?? ""),
            new KeyValuePair<string,string>("audience", "wompi_api")
        });

        var response = await _httpClient.PostAsync(
            "https://id.wompi.sv/connect/token",
            content
        );

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error obteniendo token de Wompi: {Error}", responseContent);
            return string.Empty;
        }

        var authResponse = JsonSerializer.Deserialize<WompiAuthResponse>(responseContent);

        if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
        {
            _logger.LogError("Token de Wompi vacío o inválido.");
            return string.Empty;
        }
        _logger.LogInformation("ClientId usado: {ClientId}", clientId);
        _logger.LogInformation("ClientSecret length: {Length}", clientSecret?.Length);
        _logger.LogInformation("Token de Wompi obtenido correctamente.");

        return authResponse.AccessToken;
    }

    // 2️⃣ Crear enlace de pago
    public async Task<WompiTransactionResponse> CrearEnlacePago(WompiTransactionRequest request)
    {
        try
        {
            var token = await ObtenerTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("No se pudo obtener el token de Wompi.");
            }

            var payload = new
            {
                identificadorEnlaceComercio = request.Referencia,
                monto = request.Monto,
                nombreProducto = "Pedido " + request.Referencia,

                formaPago = new
                {
                    permitirTarjetaCreditoDebido = true
                },

                configuracion = new
                {
                    urlRedirect = request.RedirectUrl,
                    esRecurrente = false,
                    notificarTransaccionCliente = true,
                    emailsNotificacion = "rodrigobenitez.ag@gmail.com"
                    
                },

                limitesDeUso = new
                {
                    cantidadMaximaPagosExitosos = 1
                }
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.wompi.sv/EnlacePago",
                payload
            );

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error Wompi SV: {Response}", responseContent);

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
                    Error = "Respuesta inválida de Wompi"
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
}
