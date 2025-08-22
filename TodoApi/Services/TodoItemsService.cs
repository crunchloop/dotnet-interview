using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services
{
    public class TodoItemService : ITodoItemService
    {
        private readonly TodoContext _context;

        public TodoItemService(TodoContext context)
        {
            _context = context;
        }

        public async Task<IList<TodoItem>> GetAllAsync(long listId)
        {
            return await _context.TodoItems
                .Where(i => i.TodoListId == listId)
                .Include(i => i.TodoList)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TodoItem?> GetByIdAsync(long listId, long itemId)
        {
            return await _context.TodoItems
                .Include(i => i.TodoList)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.TodoListId == listId); 
        }

        public async Task<TodoItem?> UpdateAsync(long listId, long id, UpdateTodoItem payload)
        {
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == id && i.TodoListId == listId); 

            if (todoItem == null) return null;

            todoItem.Title = payload.Title;
            todoItem.IsCompleted = payload.IsCompleted;

            await _context.SaveChangesAsync();
            return todoItem;
        }

        public async Task<TodoItem?> CreateAsync(long listId, CreateTodoItem payload)
        {
            var list = await _context.TodoList.FindAsync(listId);
            if (list == null) return null;

            var todoItem = new TodoItem
            {
                Title = payload.Title,
                TodoListId = listId
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return todoItem;
        }

        public async Task<bool> DeleteAsync(long listId, long id)
        {
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == id && i.TodoListId == listId);

            if (todoItem == null) return false;

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
