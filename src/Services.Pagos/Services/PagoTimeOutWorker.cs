using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Services.Pagos.Services;

public class PagoTimeoutWorker : BackgroundService
{
    private const int EstadoPendienteLegacy = 0;
    private const int EstadoPendienteActual = 1;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<PagoTimeoutWorker> _logger;

    public PagoTimeoutWorker(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<PagoTimeoutWorker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var inventarioUrl = _config["Services:InventarioBaseUrl"] ?? "http://inventario-service:8080";
        var timeoutMinutes = _config.GetValue<int?>("Payments:PendingTimeoutMinutes") ?? 5;
        var pollingSeconds = _config.GetValue<int?>("Payments:PendingPollingSeconds") ?? 30;
        var motivoRechazo = _config["Payments:TimeoutRejectReason"] ?? "Timeout sin confirmacion de Wompi";

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{inventarioUrl}/api/pagos", stoppingToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se pudo obtener pagos pendientes. Status: {StatusCode}", response.StatusCode);
                    await Task.Delay(TimeSpan.FromSeconds(pollingSeconds), stoppingToken);
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync(stoppingToken);
                var pagos = JsonSerializer.Deserialize<List<PagoDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<PagoDto>();

                var ahora = DateTime.UtcNow;

                foreach (var pago in pagos.Where(EsPendiente))
                {
                    if (string.IsNullOrWhiteSpace(pago.ReferenciaTransaccion))
                    {
                        continue;
                    }

                    var tiempoPendiente = ahora - pago.FechaPago;
                    if (tiempoPendiente.TotalMinutes <= timeoutMinutes)
                    {
                        continue;
                    }

                    var rechazarResponse = await client.PostAsJsonAsync(
                        $"{inventarioUrl}/api/pagos/marcar-rechazado/{Uri.EscapeDataString(pago.ReferenciaTransaccion)}",
                        new { motivo = motivoRechazo },
                        cancellationToken: stoppingToken);

                    if (rechazarResponse.IsSuccessStatusCode || rechazarResponse.StatusCode == HttpStatusCode.Conflict)
                    {
                        _logger.LogInformation(
                            "Pago rechazado por timeout. Ref: {Ref}. TiempoPendienteMin: {Minutos}",
                            pago.ReferenciaTransaccion,
                            Math.Round(tiempoPendiente.TotalMinutes, 2));
                        continue;
                    }

                    var detalle = await rechazarResponse.Content.ReadAsStringAsync(stoppingToken);
                    _logger.LogWarning(
                        "No se pudo rechazar pago por timeout. Ref: {Ref}. Status: {Status}. Detalle: {Detalle}",
                        pago.ReferenciaTransaccion,
                        rechazarResponse.StatusCode,
                        detalle);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en PagoTimeoutWorker");
            }

            await Task.Delay(TimeSpan.FromSeconds(pollingSeconds), stoppingToken);
        }
    }

    private static bool EsPendiente(PagoDto pago)
    {
        return pago.EstadoPago == EstadoPendienteActual || pago.EstadoPago == EstadoPendienteLegacy;
    }

    private sealed class PagoDto
    {
        public string ReferenciaTransaccion { get; set; } = string.Empty;
        public int EstadoPago { get; set; }
        public DateTime FechaPago { get; set; }
    }
}
