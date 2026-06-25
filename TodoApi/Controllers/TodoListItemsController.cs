using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/todolists/{todoListId}/items")]
    [ApiController]
    public class TodoListItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoListItemsController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/todolists/5/items
        [HttpGet]
        public async Task<ActionResult<IList<TodoListItem>>> GetTodoListItems(long todoListId)
        {
            if (!await TodoListExists(todoListId))
            {
                return NotFound();
            }

            return Ok(
                await _context
                    .TodoListItem.Where(item => item.TodoListId == todoListId)
                    .ToListAsync()
            );
        }

        // GET: api/todolists/5/items/3
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoListItem>> GetTodoListItem(long todoListId, long id)
        {
            var todoListItem = await _context.TodoListItem.FirstOrDefaultAsync(item =>
                item.Id == id && item.TodoListId == todoListId
            );

            if (todoListItem == null)
            {
                return NotFound();
            }

            return Ok(todoListItem);
        }

        // PUT: api/todolists/5/items/3
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult> PutTodoListItem(
            long todoListId,
            long id,
            UpdateTodoListItem payload
        )
        {
            var todoListItem = await _context.TodoListItem.FirstOrDefaultAsync(item =>
                item.Id == id && item.TodoListId == todoListId
            );

            if (todoListItem == null)
            {
                return NotFound();
            }

            todoListItem.Name = payload.Name;
            todoListItem.IsComplete = payload.IsComplete;
            await _context.SaveChangesAsync();

            return Ok(todoListItem);
        }

        // POST: api/todolists/5/items
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoListItem>> PostTodoListItem(
            long todoListId,
            CreateTodoListItem payload
        )
        {
            if (!await TodoListExists(todoListId))
            {
                return NotFound();
            }

            var todoListItem = new TodoListItem
            {
                Name = payload.Name,
                IsComplete = payload.IsComplete,
                TodoListId = todoListId,
            };

            _context.TodoListItem.Add(todoListItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetTodoListItem",
                new { todoListId, id = todoListItem.Id },
                todoListItem
            );
        }

        // DELETE: api/todolists/5/items/3
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoListItem(long todoListId, long id)
        {
            var todoListItem = await _context.TodoListItem.FirstOrDefaultAsync(item =>
                item.Id == id && item.TodoListId == todoListId
            );
            if (todoListItem == null)
            {
                return NotFound();
            }

            _context.TodoListItem.Remove(todoListItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> TodoListExists(long todoListId)
        {
            return await _context.TodoList.AnyAsync(list => list.Id == todoListId);
        }
    }
}
