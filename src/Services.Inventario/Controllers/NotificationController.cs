using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Inventario.Hubs;
using Shared.Core.Entities;

namespace Service.Inventario.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificacionController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] Notificacion notificacion)
        {
            // Validamos que el objeto no venga vacío
            if (notificacion == null) return BadRequest("Datos de notificación inválidos");

            // Disparamos el mensaje a través del Hub
            // El nombre "ReceiveNotification" debe ser igual al que usa tu Dashboard.jsx
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