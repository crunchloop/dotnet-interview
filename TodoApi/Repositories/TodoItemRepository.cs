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
            var itemsFromList = this._context.TodoItem.Where(x => x.TodoListId == listId).ToList();

            if (itemsFromList.Count < 0)
            {
                return null;
            }

            return itemsFromList;
        }

        public async Task CreateItem(TodoItem item)
        {
            this._context.TodoItem.Add(item);
            await this._context.SaveChangesAsync();
        }
    }
}
