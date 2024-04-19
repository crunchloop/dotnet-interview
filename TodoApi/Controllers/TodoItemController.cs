using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoApi.Controllers
{
    [Route("api/todolist/{idList:long}/todoitem")]
    [ApiController]
    public class TodoItemController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoItemController(TodoContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id, long idList)
        {
            var list = await _context.TodoList.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == idList);

            if (list == null)
                return NotFound();

            var item = list.Items?.FirstOrDefault(i => i.Id == id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // POST api/<TodoItemController>
        [HttpPost]
        public async Task<ActionResult<TodoItem>> Post([FromBody] TodoItem item, long idList)
        {
            if (_context.TodoList == null)
                return Problem("Entity set 'TodoContext.TodoList'  is null.");

            var list = await _context.TodoList.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == idList);

            if (list == null)
                return NotFound();

            list.Items?.Add(item);
            _context.Update(list);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Problem("Error saving in db.");
            }

            return CreatedAtAction( "GetTodoItem", new { id = item.Id, idList = list.Id }, item);
        }

        // PUT api/<TodoItemController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, long idList, TodoItem item)
        {
            if (id != item.Id)
                return BadRequest();

            var existingItem = _context.TodoItems.Local.FirstOrDefault(i => i.Id == item.Id);

            if (existingItem != null)
            {
                _context.Entry(existingItem).State = EntityState.Detached; // Desasociar la entidad existente
            }

            var itemDb = await _context.TodoItems.FirstOrDefaultAsync(i => i.Id == id);

            if (itemDb == null)
                return NotFound();

            itemDb.Title = item.Title;
            itemDb.Description = item.Description;
            itemDb.Completed = item.Completed;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                return StatusCode(500, $"Error updating the object: {e.Message}");
            }

            return Ok();
        }


        // DELETE api/<TodoItemController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id, long idList)
        {
            var list = await _context.TodoList.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == idList);

            if (list == null)
                return NotFound();

            var item = list.Items?.FirstOrDefault(i => i.Id == id);

            if (item == null)
                return NotFound();

            try
            {
                list.Items?.Remove(item);

                _context.Update(list);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, $"Error saving the change: {e.Message}");
            }

            return Ok();
        }

    }
}
