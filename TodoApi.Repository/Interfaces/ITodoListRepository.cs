using TodoApi.Models;

namespace TodoApi.Repository.Interfaces
{
    public interface ITodoListRepository
    {
        Task<ICollection<TodoList>> GetAll();

        Task<TodoList> Get(long id);

        Task<long> Create(TodoList list);

        Task Update(long id, TodoList list);

        Task Delete(long id);
    }
}
