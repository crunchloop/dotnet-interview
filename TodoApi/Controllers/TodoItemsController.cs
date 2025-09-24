using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/todolists/{todoListId}/items")]
    [ApiController]

    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoItemsController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/todolists/5/items/5
        [HttpGet]
        public async Task<ActionResult<IList<TodoItem>>> GetTodoItemsByTodoList([FromRoute] long todoListId)
        {
            var todoItems = await _context.TodoItem.Where(t => t.TodoListId == todoListId).ToListAsync();
            return Ok(todoItems);
        }

        // GET: api/todolists/5/items/5
        [HttpGet("{todoItemId}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem([FromRoute] long todoItemId)
        {
            var todoItem = await _context.TodoItem.FindAsync(todoItemId);

            if (todoItem == null)
            {
                return NotFound();
            }

            return Ok(todoItem);
        }

        // PUT: api/todolists/5/items/1
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{todoItemId}")]
        public async Task<ActionResult> PutTodoItem([FromRoute] long todoItemId, UpdateTodoItem payload)
        {
            var todoItem = await _context.TodoItem.FindAsync(todoItemId);
            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.Name = payload.Name;
            todoItem.IsComplete = payload.IsComplete;
            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }

        // PUT: api/todolists/5/items/1
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPatch("{todoItemId}")]
        public async Task<ActionResult> CompleteTodoItem([FromRoute] long todoItemId, CompleteTodoItem completeTodoItem)
        {
            var todoItem = await _context.TodoItem.FindAsync(todoItemId);
            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.IsComplete = completeTodoItem.IsComplete;
            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }

        // POST: api/todolists/1/items
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoList>> PostTodoList([FromRoute] long todoListId, CreateTodoItem payload)
        {
            // Validates associated TodoList exists.
            var todoList = await _context.TodoList.FindAsync(todoListId);
            if (todoList == null)
            {
                return NotFound();
            }

            // Persists the TodoItem.
            var todoItem = new TodoItem
            {
                Name = payload.Name,
                TodoListId = todoListId
            };
            _context.TodoItem.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItem), new { todoListId = todoListId, todoItemId = todoItem.Id }, todoItem);
        }

        // DELETE: api/todolists/5/items/1
        [HttpDelete("{todoItemId}")]
        public async Task<ActionResult> DeleteTodoItem([FromRoute] long todoItemId)
        {
            var todoItem = await _context.TodoItem.FindAsync(todoItemId);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItem.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
