using Microsoft.AspNetCore.SignalR;
using TodoApi.Application.Interfaces;

namespace TodoApi.Application.Hubs
{
    public class NotificationHub : Hub<INotificationHub>
    {
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;

            await Clients.Client(connectionId).Pong();

            await base.OnConnectedAsync();
        }
    }
}
