using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Services.Interfaces
{
    public interface ITodoItemService
    {
        Task<IList<TodoItem>> GetAllAsync(long listId);
        Task<TodoItem?> GetByIdAsync(long listId, long itemId);
        Task<TodoItem?> CreateAsync(long listId, CreateTodoItem payload);
        Task<TodoItem?> UpdateAsync(long listId, long itemId, UpdateTodoItem payload);
        Task<bool> DeleteAsync(long listId, long itemId);
    }
}
