using Services.ChatBot.DTOs;
using Shared.Core.Entities;
using Telegram.Bot.Types.Enums;
using System.Text.Json;

namespace Webhook.Controllers.Services;

/// <summary>
/// Service responsible for communicating with the Payments microservice to handle 
/// transaction-related tasks such as generating payment links.
/// </summary>
/// <param name="httpClientFactory">The factory used to create HTTP clients for inter-service communication.</param>
/// <param name="logger">The logger instance for tracking diagnostic data and errors.</param>
public class PaymentService(
IHttpClientFactory httpClientFactory, 
ILogger<PaymentService> logger
)
{
    /// <summary>The factory used to create HTTP clients.</summary>
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    /// <summary>The logger instance.</summary>
    private readonly ILogger<PaymentService> _logger = logger;

    /// <summary>
    /// Communicates with the Pagos API to generate an automatic payment link for a specific order.
    /// </summary>
    /// <param name="idPedido">The unique identifier of the order.</param>
    /// <returns>A <see cref="PagosLinksDTO"/> containing the link and reference if successful; otherwise, null.</returns>
    public async Task<PagosLinksDTO?> GeneratedPaymentLink(int idPedido)
    {
        var client = _httpClientFactory.CreateClient("PagosApi");
        //Console.WriteLine($"\n\nId Pedido en Pagos:" + idPedido.ToString());
        var url = $"api/pagos/crear-enlace-automatico/{idPedido}";
        try
        {
            var res = await client.PostAsync(url, null);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadFromJsonAsync<PagosLinksDTO>(
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );
                return data;
            }
            _logger.LogError($"Error en Pagos Service: {res.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error en Pagos Service cat: {ex.Message}");
            return null;
        }
    }
}