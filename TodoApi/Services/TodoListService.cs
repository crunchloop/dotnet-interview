using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services
{
    public class TodoListService : ITodoListService
    {
        private readonly TodoContext _context;

        public TodoListService(TodoContext context)
        {
            _context = context;
        }

        public async Task<IList<TodoList>> GetAllAsync()
        {
            return await _context.TodoList.AsNoTracking().ToListAsync();
        }

        public async Task<TodoList?> GetByIdAsync(long id)
        {
            return await _context.TodoList.FindAsync(id);
        }

        public async Task<TodoList> CreateAsync(CreateTodoList payload)
        {
            var todoList = new TodoList { Name = payload.Name };
            _context.TodoList.Add(todoList);
            await _context.SaveChangesAsync();
            return todoList;
        }

        public async Task<TodoList?> UpdateAsync(long id, UpdateTodoList payload)
        {
            var todoList = await _context.TodoList.FindAsync(id);
            if (todoList == null) return null;

            todoList.Name = payload.Name;
            await _context.SaveChangesAsync();
            return todoList;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var todoList = await _context.TodoList.FindAsync(id);
            if (todoList == null) return false;

            _context.TodoList.Remove(todoList);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.TodoList.AnyAsync(e => e.Id == id);
        }
    }
}
