using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repository.Interfaces;

namespace TodoApi.Repository.Implementations
{
    public class TodoItemRepository : ITodoItemRepository, IDisposable
    {
        private readonly TodoContext _context;

        public TodoItemRepository(TodoContext context)
        {
            _context = context;
        }

        public async Task<TodoItem> Get(long idList, long idItem) {
            var list = await _context.TodoList.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == idList);

            if (list == null)
                throw new KeyNotFoundException();

            var item = list.Items?.FirstOrDefault(i => i.Id == idItem);

            if (item == null)
                throw new KeyNotFoundException();

            return item;
        }

        public async Task Delete(long idList, long idItem) {

            var list = await _context.TodoList.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == idList);

            if (list == null)
                throw new KeyNotFoundException();

            var item = list.Items?.FirstOrDefault(i => i.Id == idItem);

            if (item == null)
                throw new KeyNotFoundException();

            try
            {
                list.Items?.Remove(item);

                _context.Update(list);

                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }

        }

        public async Task Update(long idItem, TodoItem item) {

            if (idItem != item.Id)
                throw new KeyNotFoundException();

            var existingItem = _context.TodoItems.Local.FirstOrDefault(i => i.Id == item.Id);

            if (existingItem != null)
            {
                _context.Entry(existingItem).State = EntityState.Detached; // Desasociar la entidad existente
            }

            var itemDb = await _context.TodoItems.FirstOrDefaultAsync(i => i.Id == idItem);

            if (itemDb == null)
                throw new ArgumentNullException();

            itemDb.Title = item.Title;
            itemDb.Description = item.Description;
            itemDb.Completed = item.Completed;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<long> Create(long idList, TodoItem item) {

            if (_context.TodoList == null)
                throw new ArgumentNullException("Entity set 'TodoContext.TodoList'  is null.");

            var list = await _context.TodoList.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == idList);

            if (list == null)
                throw new ArgumentNullException();

            list.Items?.Add(item);
            _context.Update(list);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }

            return item.Id;
        }

        public void Dispose() {
            _context.Dispose();
        }

    }
}
