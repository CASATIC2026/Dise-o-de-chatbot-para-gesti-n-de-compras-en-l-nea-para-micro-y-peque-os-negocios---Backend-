using Services.ChatBot.DTOs;
using Shared.Core.Entities;
using Telegram.Bot.Types.Enums;
using System.Text.Json;

namespace Webhook.Controllers.Services;

public class PaymentService(
IHttpClientFactory httpClientFactory, 
ILogger<PaymentService> logger
)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<PaymentService> _logger = logger;

    public async Task<PagosLinksDTO?> GeneratedPaymentLink(int idPedido)
    {
        var client = _httpClientFactory.CreateClient("PagosApi");
        Console.WriteLine($"\n\nId Pedido en Pagos:" + idPedido.ToString());
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