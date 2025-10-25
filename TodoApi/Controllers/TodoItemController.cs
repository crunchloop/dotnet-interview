using Microsoft.AspNetCore.Mvc;
using TodoApi.Application.Dtos;
using TodoApi.Application.Interfaces;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/todoList/{listId}/todoItem")]
    [ApiController]
    [ServiceFilter(typeof(TodoListExistsFilter))]
    public class TodoItemController : ControllerBase
    {
        private readonly ITodoItemService _todoItemService; 

        public TodoItemController(ITodoItemService todoItemService)
        {
            _todoItemService = todoItemService;
        }

        // GET: api/todoList/{listId}/todoItem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetItems(long listId)
        {
            var items = await _todoItemService.GetAllItems(listId);

            return items != null ? Ok(items) : NoContent();
        }

        // GET: api/todoList/{listId}/todoItem/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetItem(long listId, int id)
        {
            var item = await _todoItemService.GetItem(listId, id);

            return item != null ? Ok(item) : NoContent();
        }

        // POST: api/todoList/{listId}/todoItem
        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateItem(long listId, CreateItemDto createRequest)
        {
            var result = await _todoItemService.CreateItem(listId, createRequest);

            return Ok(result);
        }

        // PUT: api/todoList/{listId}/todoItem/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(long listId, int id, UpdateItemDto updateRequest)
        {
            var result = await _todoItemService.UpdateItem(listId, id, updateRequest);
            return Ok(result);
        }

        // DELETE: api/todoList/{listId}/todoItem/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(long listId, int id)
        {
            var result = await _todoItemService.DeleteItem(listId, id);

            return Ok(result);
        }
    }
}