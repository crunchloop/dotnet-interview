using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Infrastructure.Interfaces;
using TodoApi.Models;

namespace TodoApi.Infrastructure.Repositories
{
    public class TodoListRepository : Repository<TodoList>, ITodoListRepository
    {
        public TodoListRepository(TodoContext dbContext) : base(dbContext) { }
        public async Task<bool> Exits(long listId)
        {
            return await _dbSet.AnyAsync(t => t.Id == listId);
        }
    }
}
