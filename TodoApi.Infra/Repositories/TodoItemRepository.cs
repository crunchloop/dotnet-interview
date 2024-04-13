using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public class TodoItemRepository : ITodoItemRepository
    {
        private readonly TodoContext _context;

        public TodoItemRepository(TodoContext context)
        {
            this._context = context;
        }

        public async Task<List<TodoItem>> GetAllItems(int listId)
        {
            var itemsFromList = await this._context.TodoItem.Where(x => x.TodoListId == listId).ToListAsync();
            return itemsFromList;
        }

        public async Task<int> CreateItem(TodoItem item)
        {
            try
            {
                this._context.TodoItem.Add(item);
                await this._context.SaveChangesAsync();
                return item.Id;
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateItem(int id, string itemName)
        {
            try
            {
                var item = await this._context.TodoItem.FirstOrDefaultAsync(x => x.Id == id);

                if (item == null)
                {
                    throw new Exception("Item not found");
                }

                item.ItemName = itemName;
                await this._context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task DeleteItem(int itemId)
        {
            try
            {
                var item = await this._context.TodoItem.FirstOrDefaultAsync(x => x.Id == itemId);

                if (item == null)
                {
                    throw new Exception("Item not found");
                }

                this._context.TodoItem.Remove(item);
                await this._context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<TodoItem> GetByIdAsync(int itemId)
        {
            var item = await this._context.TodoItem.FirstOrDefaultAsync(x => x.Id == itemId);
            return item;
        }
    }
}
