using TodoApi.Models;

namespace TodoApi.Infrastructure.Interfaces
{
    public interface ITodoItemRepository
    {
        Task<IEnumerable<TodoItem>> GetAllByListIdAsync(long listId);
        Task<TodoItem?> GetByIdAndListIdAsync(int id, long listId);
        Task<int> Create(TodoItem item);
        Task<bool> ExitstenceByNameAndListIdAsync(long listId, string name);
        Task Update(TodoItem item);
        Task Delete(int itemId);
        Task<IEnumerable<TodoItem>> GetAllItemsNotComplete(long listId);
    }
}
