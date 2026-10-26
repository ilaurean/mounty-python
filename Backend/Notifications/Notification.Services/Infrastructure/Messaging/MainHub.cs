using Inventory.Contracts;
using Microsoft.AspNetCore.SignalR;
using Operative.Contracts;
using ILogger = Serilog.ILogger;

namespace Notification.Services.Infrastructure.Messaging;

public interface IPushNotificationService
{
    Task UpdatedBoard(BoardDTO board);
    Task UpdatedOrder(OrderDTO order);

}
public class PushNotificationService(IHubContext<MainHub> hub) : IPushNotificationService
{
    public async Task UpdatedBoard(BoardDTO board)
    {
        await hub.Clients.All.SendAsync("Board", board);
    }

    public async Task UpdatedOrder(OrderDTO order)
    {
        await hub.Clients.All.SendAsync("Order", order);
    }
}

public class MainHub(ILogger logger) : Hub
{
    public async Task SendMessage(string user, string message)
    {
        logger.Information("MainHub: send message: " + message);
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public override Task OnConnectedAsync()
    {
        logger.Information("MainHub: new connection");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.Information("MainHub: disconnection");
        return base.OnDisconnectedAsync(exception);
    }
}
