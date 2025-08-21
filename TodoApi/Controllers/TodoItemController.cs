using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// DTOs separados por carpetas
using TodoApi.Dtos.Request;
using TodoApi.Dtos.Response;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    // ruta simplificada para los items dentro de una todoList
    [Route("api/todolists/{todoListId}/items")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        public TodoItemsController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/todolists/{todoListId}/items
        [HttpGet]
        public async Task<ActionResult<IList<TodoItemResponseDto>>> GetTodoItems(long todoListId)
        {
            try
            {
                var todoList = await _context.TodoList
                    .Include(tl => tl.Items)
                    .FirstOrDefaultAsync(todoItems => todoItems.Id == todoListId);

                if (todoList == null)
                    return NotFound($"No todo list found for ID {todoListId}.");

                IList<TodoItem> items = todoList.Items;

                return Ok(items.Select(item => MapToDto(item)));
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Error retrieving todo items",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        // GET: api/todolists/{todoListId}/items/{todoItemId}
        [HttpGet("{todoItemId}")]
        public async Task<ActionResult<TodoItemResponseDto>> GetTodoItem(long todoListId, long todoItemId)
        {
            try
            {
                var todoList = await _context.TodoList
                    .Include(tl => tl.Items)
                    .FirstOrDefaultAsync(todoItems => todoItems.Id == todoListId);

                if (todoList == null)
                {
                    return NotFound($"No todo list found for ID {todoListId}");
                }

                var todoItem = todoList.Items.FirstOrDefault(item => item.Id == todoItemId);

                if (todoItem == null)
                {
                    return NotFound("No item found with the specified ID.");
                }

                return Ok(MapToDto(todoItem));
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Error retrieving todo item",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        // POST: api/todolists/{todoListId}/items/add
        [HttpPost("add")]
        public async Task<ActionResult<TodoItemResponseDto>> CreateTodoItem([FromBody] CreateTodoItem todoItem)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (todoItem == null)
                {
                    return BadRequest("UpdateTodoItem cannot be null.");
                }

                var todoList = await _context.TodoList
                    .Include(todoList => todoList.Items)
                    .FirstOrDefaultAsync(tl => tl.Id == todoItem.TodoListId);

                if (todoList == null)
                {
                    return BadRequest($"No todo list with ID {todoItem.TodoListId} was found");
                }

                if (todoList.Items.Any(item => item.Name == todoItem.Name))
                {
                    return Conflict($"There's already a todo item with the same name!");
                }

                TodoItem todoListItem = new TodoItem()
                {
                    Name = todoItem.Name,
                    IsComplete = todoItem.IsComplete,
                    TodoListId = todoItem.TodoListId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                todoList.Items.Add(todoListItem);

                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    actionName: nameof(GetTodoItem),
                    routeValues: new {todoListId = todoList.Id, todoItemId = todoListItem.Id},
                    value: MapToDto(todoListItem)
                );
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Error creating todo item",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        // PUT: api/todolists/{todoListId}/items/update
        [HttpPut("update")]
        public async Task<ActionResult<TodoItemResponseDto>> UpdateTodoItem([FromBody] UpdateTodoItem updateTodoItem)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (updateTodoItem == null)
                {
                    return BadRequest("UpdateTodoItem cannot be null.");
                }

                TodoItem? todoItem = await _context.TodoItem
                    .Include(todoItem => todoItem.TodoList)
                    .FirstOrDefaultAsync(todoItem => todoItem.Id == updateTodoItem.Id);

                if (todoItem == null)
                {
                    return NotFound($"No todo item found with ID {updateTodoItem.Id}.");
                }

                todoItem.Name = updateTodoItem.Name;
                todoItem.IsComplete = updateTodoItem?.IsComplete ?? todoItem.IsComplete;
                todoItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(MapToDto(todoItem));
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Error updating todo item",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        // DELETE: api/todolists/{todoListId}/items/
        [HttpDelete]
        public async Task<ActionResult> DeleteTodoItems(int todoListId)
        {
            try
            {
                TodoList? todoList = await _context.TodoList
                    .Include(TodoList => TodoList.Items)
                    .FirstOrDefaultAsync(ti => ti.Id == todoListId);

                if (todoList == null)
                {
                    return NotFound($"No todo list found with ID {todoList}.");
                }

                if (todoList.Items.Count == 0)
                    return Ok("Todo list is already empty!");

                foreach (TodoItem item in todoList.Items)
                {
                    _context.TodoItem.Remove(item);
                }

                await _context.SaveChangesAsync();

                return Ok($"Todo items deleted successfully!");
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "There was a problem trying to delete todo items",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        // DELETE: api/todolists/{todoListId}/items/{todoItemId}
        [HttpDelete("{todoItemId}")]
        public async Task<ActionResult> DeleteTodoItem(int todoListId, long todoItemId)
        {
            try
            {
                TodoItem? todoItem = await _context.TodoItem
                    .FirstOrDefaultAsync(ti => ti.Id == todoItemId && ti.TodoListId == todoListId);

                if (todoItem == null)
                {
                    return NotFound($"No todo item found with ID {todoItemId}.");
                }

                _context.TodoItem.Remove(todoItem);

                await _context.SaveChangesAsync();

                return Ok($"Todo item with ID {todoItemId} deleted successfully!");
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "There was a problem trying to delete todo item",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        private static TodoItemResponseDto MapToDto(TodoItem todoItem)
        {
            return new TodoItemResponseDto()
            {
                Id = todoItem.Id,
                Name = todoItem.Name,
                IsComplete = todoItem.IsComplete,
                TodoListId = todoItem.TodoListId
            };
        }
            
    }
}
