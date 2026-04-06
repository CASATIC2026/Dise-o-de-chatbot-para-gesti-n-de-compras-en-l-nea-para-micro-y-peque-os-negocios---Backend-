using Microsoft.AspNetCore.SignalR;

namespace Service.Inventario.Hubs
{
    // El Hub es el "túnel" de comunicación en tiempo real
    public class NotificationHub : Hub
    {
        // Método para que el servidor envíe alertas al Dashboard
        public async Task SendNotification(object notification)
        {
            // Envía el objeto a todos los clientes conectados (Admin Panel)
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }

        // Opcional: Log de conexión para depuración
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"--> Administrador conectado: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
    }
}