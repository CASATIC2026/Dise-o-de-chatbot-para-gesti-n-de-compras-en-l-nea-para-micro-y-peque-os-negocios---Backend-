using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Services.Pagos.Services;

/// <summary>
/// A background service that periodically monitors pending payments and marks them as rejected 
/// if they exceed the configured timeout duration without receiving a confirmation.
/// </summary>
public class PagoTimeoutWorker : BackgroundService
{
    private const int EstadoPendienteLegacy = 0;
    private const int EstadoPendienteActual = 1;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<PagoTimeoutWorker> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagoTimeoutWorker"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory to create HTTP clients for inter-service communication.</param>
    /// <param name="config">Configuration provider for timeout and polling settings.</param>
    /// <param name="logger">Logger instance for tracking worker activities.</param>
    public PagoTimeoutWorker(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<PagoTimeoutWorker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background task logic, polling the inventory service for pending payments 
    /// and applying timeout rules.
    /// </summary>
    /// <param name="stoppingToken">Triggered when the host is shutting down.</param>
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

                    if (rechazarResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            "Pago rechazado por timeout. Ref: {Ref}. TiempoPendienteMin: {Minutos}",
                            pago.ReferenciaTransaccion,
                            Math.Round(tiempoPendiente.TotalMinutes, 2));
                        continue;
                    }

                    var detalle = await rechazarResponse.Content.ReadAsStringAsync(stoppingToken);
                    if (rechazarResponse.StatusCode == HttpStatusCode.Conflict)
                    {
                        _logger.LogInformation(
                            "No se marco pago por timeout porque ya esta en estado final. Ref: {Ref}. Detalle: {Detalle}",
                            pago.ReferenciaTransaccion,
                            detalle);
                        continue;
                    }

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

    /// <summary>
    /// Helper method to determine if a payment is in a pending state, 
    /// accounting for both legacy and current status codes.
    /// </summary>
    /// <param name="pago">The payment data transfer object.</param>
    /// <returns>True if the payment is pending; otherwise, false.</returns>
    private static bool EsPendiente(PagoDto pago)
    {
        var estado = pago.EstadoPago ?? pago.Estado;
        return estado == EstadoPendienteActual || estado == EstadoPendienteLegacy;
    }

    /// <summary>
    /// Internal Data Transfer Object used to deserialize payment data from the Inventory service.
    /// </summary>
    private sealed class PagoDto
    {
        /// <summary>The unique transaction reference.</summary>
        public string ReferenciaTransaccion { get; set; } = string.Empty;
        /// <summary>The order/payment state (legacy field).</summary>
        public int? Estado { get; set; }
        /// <summary>The explicit payment state.</summary>
        public int? EstadoPago { get; set; }
        /// <summary>The timestamp when the payment record was last initialized or updated.</summary>
        public DateTime FechaPago { get; set; }
    }
}
