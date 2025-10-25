using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Infrastructure.Interfaces;
using TodoApi.Infrastructure.Repositories;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public class TodoItemRepository : Repository<TodoItem>, ITodoItemRepository
    {
        public TodoItemRepository(TodoContext dbContext) : base(dbContext)
        {
        }

        public async Task<int> Create(TodoItem item)
        {
            var itemCreated = await AddAsync(item);
            return itemCreated.Id;
        }

        public async Task<IEnumerable<TodoItem>> GetAllByListIdAsync(long listId)
        {
            return await _dbSet
                .Where(t => t.TodoListId == listId)
                .ToListAsync();
        }

        public async Task<TodoItem?> GetByIdAndListIdAsync(int id, long listId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Id == id && t.TodoListId == listId);
        }

        public async Task<bool> ExitstenceByNameAndListIdAsync(long listId, string name)
        {
            return await _dbSet
               .Where(t => t.Name.ToLower() == name.ToLower() && t.TodoListId == listId)
               .AnyAsync();
        }

        public async Task Update(TodoItem item)
        {
            await UpdateAsync(item);
        }

        public async Task Delete(int itemId)
        {
            await DeleteAsync(itemId);
        }

        public async Task<IEnumerable<TodoItem>> GetAllItemsNotComplete(long listId)
        {
            return await _dbSet.Where(i => i.TodoListId == listId && !i.Complete).ToListAsync();
        }
    }
}
