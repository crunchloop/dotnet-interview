using TodoApi.Application.ViewModels;
using TodoApi.Models;

namespace TodoApi.Application.Middlewares.Contracts
{
    public interface ITodoItemMiddleware
    {
        Task<List<TodoItemViewModel>> GetAllItems(int listId);

        Task<int> CreateItem(CreateTodoItemViewModel item);

        Task UpdateItem(int itemId, UpdateTodoItemViewModel item);

        Task DeleteItem(int itemId);
    }
}
