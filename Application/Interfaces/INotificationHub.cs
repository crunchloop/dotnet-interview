using TodoApi.Application.Dtos;

namespace TodoApi.Application.Interfaces
{
    public interface INotificationHub
    {
        Task ItemCompleted(NotificationItem item);
        Task Ping();
        Task Pong();
    }
}
