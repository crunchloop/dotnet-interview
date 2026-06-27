using MediatR;
using Microsoft.AspNetCore.Mvc;
using Todo.Application.Common.Dtos;
using Todo.Application.TodoListItems.Commands.CreateTodoListItem;
using Todo.Application.TodoListItems.Commands.DeleteTodoListItem;
using Todo.Application.TodoListItems.Commands.UpdateTodoListItem;
using Todo.Application.TodoListItems.Queries.GetTodoListItemById;
using Todo.Application.TodoListItems.Queries.GetTodoListItems;
using Todo.Web.Contracts;

namespace Todo.Web.Controllers
{
    [Route("api/todolists/{todoListId}/items")]
    [ApiController]
    public class TodoListItemsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TodoListItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/todolists/5/items
        [HttpGet]
        public async Task<ActionResult<List<TodoListItemDto>>> GetTodoListItems(long todoListId)
        {
            var items = await _mediator.Send(new GetTodoListItemsQuery { TodoListId = todoListId });

            if (items == null)
            {
                return NotFound();
            }

            return Ok(items);
        }

        // GET: api/todolists/5/items/3
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoListItemDto>> GetTodoListItem(long todoListId, long id)
        {
            var todoListItem = await _mediator.Send(
                new GetTodoListItemByIdQuery { Id = id, TodoListId = todoListId }
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
        public async Task<ActionResult<TodoListItemDto>> PutTodoListItem(
            long todoListId,
            long id,
            UpdateTodoListItemRequest payload
        )
        {
            await _mediator.Send(
                new UpdateTodoListItemCommand
                {
                    Id = id,
                    TodoListId = todoListId,
                    Name = payload.Name,
                    IsComplete = payload.IsComplete,
                }
            );

            return Ok(
                await _mediator.Send(
                    new GetTodoListItemByIdQuery { Id = id, TodoListId = todoListId }
                )
            );
        }

        // POST: api/todolists/5/items
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoListItemDto>> PostTodoListItem(
            long todoListId,
            CreateTodoListItemRequest payload
        )
        {
            var id = await _mediator.Send(
                new CreateTodoListItemCommand
                {
                    TodoListId = todoListId,
                    Name = payload.Name,
                    IsComplete = payload.IsComplete,
                }
            );

            var todoListItem = await _mediator.Send(
                new GetTodoListItemByIdQuery { Id = id, TodoListId = todoListId }
            );

            return CreatedAtAction("GetTodoListItem", new { todoListId, id }, todoListItem);
        }

        // DELETE: api/todolists/5/items/3
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoListItem(long todoListId, long id)
        {
            await _mediator.Send(
                new DeleteTodoListItemCommand { Id = id, TodoListId = todoListId }
            );

            return NoContent();
        }
    }
}
