using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Inventario.Hubs;
using Shared.Core.Entities;

namespace Service.Inventario.Controllers
{
    /// <summary>
    /// API Controller for manually broadcasting notifications to connected clients via SignalR.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificacionController"/> class.
        /// </summary>
        /// <param name="hubContext">The SignalR hub context used for sending notifications.</param>
        public NotificacionController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Sends a specific notification to all clients connected to the SignalR hub.
        /// </summary>
        /// <param name="notificacion">The notification entity containing details like Title, Message, and Type.</param>
        /// <returns>An <see cref="IActionResult"/> indicating if the notification was successfully dispatched.</returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] Notificacion notificacion)
        {
            // Validamos que el objeto no venga vacío
            if (notificacion == null) return BadRequest("Datos de notificación inválidos");

            // Disparamos el mensaje a través del Hub            
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                id = new Random().Next(1, 1000),
                titulo = notificacion.Titulo,
                mensaje = notificacion.Mensaje,
                tipo = notificacion.Tipo ?? "info",
                fecha = DateTime.UtcNow
            });

            return Ok(new { status = "Enviado", mensaje = "Alerta enviada al Dashboard" });
        }
    }
}