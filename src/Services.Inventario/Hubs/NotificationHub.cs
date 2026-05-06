using Microsoft.AspNetCore.SignalR;

namespace Service.Inventario.Hubs
{
    /// <summary>
    /// SignalR Hub that serves as the communication tunnel for real-time notifications 
    /// between the backend services and the administration dashboard.
    /// </summary>
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Broadcasts a notification object to all currently connected clients.
        /// </summary>
        /// <param name="notification">The notification payload to be sent to the clients.</param>
        /// <returns>A task that represents the asynchronous broadcast operation.</returns>
        public async Task SendNotification(object notification)
        {
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }

        /// <summary>
        /// Called when a new connection is established with the hub.
        /// Useful for logging connection activity and tracking active dashboard sessions.
        /// </summary>
        /// <returns>A task that represents the asynchronous connection event.</returns>
        public override async Task OnConnectedAsync()
        {
            // Note: In a production environment, consider using ILogger instead of Console.WriteLine
            Console.WriteLine($"--> Administrador conectado: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
    }
}