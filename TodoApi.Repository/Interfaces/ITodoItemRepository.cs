using TodoApi.Models;

namespace TodoApi.Repository.Interfaces
{
    public interface ITodoItemRepository
    {
        Task<TodoItem> Get(long idList, long idItem);

        Task<long> Create(long idList, TodoItem item);

        Task Delete(long idList, long idItem);

        Task Update(long idItem, TodoItem item);
    }
}
