using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Services.Interfaces
{
    public interface ITodoListService
    {
        Task<IList<TodoList>> GetAllAsync();
        Task<TodoList?> GetByIdAsync(long id);
        Task<TodoList> CreateAsync(CreateTodoList payload);
        Task<TodoList?> UpdateAsync(long id, UpdateTodoList payload);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
    }
}
