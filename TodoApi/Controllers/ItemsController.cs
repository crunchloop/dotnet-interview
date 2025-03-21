using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        public ItemsController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/items
        [HttpGet]
        public async Task<ActionResult<IList<Item>>> GetItems()
        {
            return Ok(await _context.Item.ToListAsync());
        }

        // GET: api/items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(long id)
        {
            Item? item = await _context.Item.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        // PUT: api/items/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult> PutItem(long id, UpdateItem payload)
        {
            Item? item = await _context.Item.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            item.Name = payload.Name;
            item.IsComplete = payload.IsComplete;

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        // POST: api/todolist/{listId}/todoItem
        [HttpPost("{listId}/todoItem")]
        public IActionResult CreateTodoItem(int listId, CreateItem newTodoItem)
        {
            TodoList? todoList = _context.TodoList.Find(listId);

            if (todoList == null)
            {
                return NotFound();
            }

            Item todoItem = new Item
            {
                Name = newTodoItem.Name,
                IsComplete = newTodoItem.IsComplete,
                TodoListId = listId
            };

            return CreatedAtAction("GetItem", new { id = todoItem.Id }, todoItem);
        }

        // DELETE: api/item/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(long id)
        {
            var item = await _context.Item.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.Item.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
