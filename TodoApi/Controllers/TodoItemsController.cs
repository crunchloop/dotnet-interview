using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services.Interfaces;

namespace TodoApi.Controllers
{
    [Route("api/todolists/{listId}/items")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemService _service;

        public TodoItemsController(ITodoItemService service)
        {
            _service = service;
        }

        // GET: api/todolists/5/items
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IList<TodoItem>>> GetTodoItems(long listId)
            => Ok(await _service.GetAllAsync(listId));

        // GET: api/todolists/5/items/10
        [Authorize]
        [HttpGet("{itemId}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long listId, long itemId)
        {
            var item = await _service.GetByIdAsync(listId, itemId);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // PUT: api/todolists/5/items/10
        [Authorize(Roles = "Admin")]
        [HttpPut("{itemId}")]
        public async Task<ActionResult<TodoItem>> PutTodoItem(long listId, long itemId, UpdateTodoItem payload)
        {
            // Validar el modelo
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(listId, itemId, payload);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // POST: api/todolists/5/items
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem([FromQuery] long listId, [FromBody] CreateTodoItem payload)
        {
            // Validar el modelo
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _service.CreateAsync(listId, payload);
            if (created == null) return NotFound("List not found");
            return CreatedAtAction(nameof(GetTodoItem), new { listId, itemId = created.Id }, created);
        }

        // DELETE: api/todolists/5/items/10
        [Authorize(Roles = "Admin")]
        [HttpDelete("{itemId}")]
        public async Task<ActionResult> DeleteTodoItem(long listId, long itemId)
        {
            var success = await _service.DeleteAsync(listId, itemId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
