namespace Ecommerce.Core.Hub;
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    /// <summary>
    /// Sends a notification message to all connected clients.
    /// boradcasting the message
    /// </summary>
    public async Task SendToAll(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
}
