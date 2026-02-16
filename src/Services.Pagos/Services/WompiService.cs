using System.Text;
using System.Text.Json;

namespace Services.Pagos.Services;

public class WompiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WompiService> _logger;
    private readonly string _publicKey;
    private readonly string _privateKey;
    private readonly string _baseUrl;

    public WompiService(HttpClient httpClient, IConfiguration configuration, ILogger<WompiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        _publicKey = _configuration["Wompi:PublicKey"] ?? throw new InvalidOperationException("Wompi PublicKey not configured");
        _privateKey = _configuration["Wompi:PrivateKey"] ?? throw new InvalidOperationException("Wompi PrivateKey not configured");
        _baseUrl = "https://sandbox.wompi.co/v1"; // Use production URL in production
    }

    public async Task<WompiTransactionResponse> CrearTransaccion(WompiTransactionRequest request)
    {
        try
        {
            var payload = new
            {
                amount_in_cents = (int)(request.Monto * 100), // Convert to cents
                currency = "COP",
                customer_email = request.Email,
                payment_method = new
                {
                    type = "CARD",
                    installments = 1
                },
                reference = request.Referencia,
                redirect_url = request.RedirectUrl
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_privateKey}");

            var response = await _httpClient.PostAsync($"{_baseUrl}/transactions", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error creating Wompi transaction: {StatusCode} - {Response}", 
                    response.StatusCode, responseContent);
                
                return new WompiTransactionResponse
                {
                    Success = false,
                    Error = $"Error al crear transacción: {response.StatusCode}"
                };
            }

            var result = JsonSerializer.Deserialize<WompiApiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Wompi transaction created: {TransactionId}", result?.Data?.Id);

            return new WompiTransactionResponse
            {
                Success = true,
                TransactionId = result?.Data?.Id ?? string.Empty,
                PaymentLink = result?.Data?.PaymentLink ?? string.Empty,
                Status = result?.Data?.Status ?? "PENDING"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating Wompi transaction");
            return new WompiTransactionResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<WompiTransactionStatus> ConsultarTransaccion(string transactionId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_publicKey}");

            var response = await _httpClient.GetAsync($"{_baseUrl}/transactions/{transactionId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error querying Wompi transaction: {StatusCode} - {Response}", 
                    response.StatusCode, responseContent);
                
                return new WompiTransactionStatus
                {
                    Success = false,
                    Error = $"Error al consultar transacción: {response.StatusCode}"
                };
            }

            var result = JsonSerializer.Deserialize<WompiTransactionQueryResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new WompiTransactionStatus
            {
                Success = true,
                TransactionId = result?.Data?.Id ?? transactionId,
                Status = result?.Data?.Status ?? "UNKNOWN",
                Reference = result?.Data?.Reference ?? string.Empty,
                AmountInCents = result?.Data?.AmountInCents ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception querying Wompi transaction: {TransactionId}", transactionId);
            return new WompiTransactionStatus
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}

// Request/Response Models
public class WompiTransactionRequest
{
    public decimal Monto { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
}

public class WompiTransactionResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string PaymentLink { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
}

public class WompiTransactionStatus
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public int AmountInCents { get; set; }
    public string? Error { get; set; }
}

// Wompi API Response Models
public class WompiApiResponse
{
    public WompiData? Data { get; set; }
}

public class WompiData
{
    public string Id { get; set; } = string.Empty;
    public string PaymentLink { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class WompiTransactionQueryResponse
{
    public WompiTransactionData? Data { get; set; }
}

public class WompiTransactionData
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public int AmountInCents { get; set; }
}
