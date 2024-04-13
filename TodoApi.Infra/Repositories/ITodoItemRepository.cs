using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface ITodoItemRepository
    {
        Task<List<TodoItem>> GetAllItems(int listId);

        Task<int> CreateItem(TodoItem item);

        Task UpdateItem(int id, string itemName);

        Task DeleteItem(int itemId);

        Task<TodoItem> GetByIdAsync(int itemId);

    }
}
