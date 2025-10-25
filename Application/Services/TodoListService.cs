using Hangfire;
using Microsoft.AspNetCore.SignalR;
using TodoApi.Application.Dtos;
using TodoApi.Application.Hubs;
using TodoApi.Application.Interfaces;
using TodoApi.Infrastructure.Interfaces;

namespace TodoApi.Application.Services
{
    public class TodoListService : ITodoListService
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IHubContext<NotificationHub, INotificationHub> _notificationHub;
        public TodoListService( ITodoListRepository todoListRepository, 
                                ITodoItemRepository todoItemRepository,
                                IHubContext<NotificationHub, INotificationHub> notificationHub) 
        {
            _todoListRepository = todoListRepository;
            _todoItemRepository = todoItemRepository;
            _notificationHub = notificationHub;
        }

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task CompleteList(long listId)
        {
            var items = await _todoItemRepository.GetAllItemsNotComplete(listId);

            foreach (var item in items)
            {
                item.Complete = true;

                await _todoItemRepository.Update(item);  

                var nI = new NotificationItem { ListId = listId, ItemId = item.Id };
               
                await _notificationHub.Clients.All.ItemCompleted(nI);

                await Task.Delay(800);

            }

        }

        public async Task<bool> ListExist(long listId)
        {
            return await _todoListRepository.Exits(listId);
        }
    }
}
