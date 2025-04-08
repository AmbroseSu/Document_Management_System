using Microsoft.AspNetCore.SignalR;

namespace Service.SignalRHub;

public class NotificationHub : Hub {
    
    public async Task SendNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }

    // Gửi thông báo đến client cụ thể
    public async Task SendToUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveMessage", message);
    }
    
}