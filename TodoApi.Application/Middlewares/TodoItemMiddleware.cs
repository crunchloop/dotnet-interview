using TodoApi.Application.Middlewares.Contracts;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Application.Middlewares
{
    public class TodoItemMiddleware : ITodoItemMiddleware
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public TodoItemMiddleware(ITodoItemRepository todoItemRepository)
        {
           this._todoItemRepository = todoItemRepository;
        }

        public async Task<List<TodoItem>> GetAllItems(int listId)
        {
            var items = await this._todoItemRepository.GetAllItems(listId);

            if(items.Count >= 0)
            {
                await Task.CompletedTask;
            }

            return items;
        }

        public async Task CreateItem(TodoItem item)
        {
            await this._todoItemRepository.CreateItem(item);
        }
    }
}
