using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface ITodoItemRepository
    {
        Task<List<TodoItem>> GetAllItems(int listId);

        Task CreateItem(TodoItem item);

    }
}
