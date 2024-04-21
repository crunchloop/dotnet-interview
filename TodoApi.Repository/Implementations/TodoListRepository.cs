using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repository.Interfaces;

namespace TodoApi.Repository.Implementations
{
    public class TodoListRepository : ITodoListRepository, IDisposable
    {
        private readonly TodoContext _context;

        public TodoListRepository(TodoContext context)
        {
            _context = context;
        }

        public async Task<ICollection<TodoList>> GetAll() {
            if (_context.TodoList == null)
                throw new NullReferenceException();

            return await _context.TodoList.ToListAsync();
        }

        public async Task<TodoList> Get(long id) {
            if (_context.TodoList == null)
                throw new NullReferenceException();

            var todoList = await _context.TodoList.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == id);

            if (todoList == null)
                throw new KeyNotFoundException();

            return todoList;
        }

        public async Task<long> Create(TodoList list) {

            if (_context.TodoList == null)
                throw new Exception("Entity set 'TodoContext.TodoList'  is null.");
            _context.TodoList.Add(list);
            await _context.SaveChangesAsync();

            return list.Id;
        }

        public async Task Delete(long id) {

            if (_context.TodoList == null)
                throw new NullReferenceException();
            var todoList = await _context.TodoList.FindAsync(id);
            if (todoList == null)
                throw new KeyNotFoundException();

            _context.TodoList.Remove(todoList);
            await _context.SaveChangesAsync();
        }

        public async Task Update(long id, TodoList list) {
            if (id != list.Id)
                throw new FormatException();

            _context.Entry(list).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
