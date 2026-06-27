using MediatR;
using Microsoft.AspNetCore.Mvc;
using Todo.Application.Common.Dtos;
using Todo.Application.TodoLists.Commands.CreateTodoList;
using Todo.Application.TodoLists.Commands.DeleteTodoList;
using Todo.Application.TodoLists.Commands.UpdateTodoList;
using Todo.Application.TodoLists.Queries.GetTodoListById;
using Todo.Application.TodoLists.Queries.GetTodoLists;
using Todo.Web.Contracts;

namespace Todo.Web.Controllers
{
    [Route("api/todolists")]
    [ApiController]
    public class TodoListsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TodoListsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/todolists
        [HttpGet]
        public async Task<ActionResult<List<TodoListDto>>> GetTodoLists()
        {
            return Ok(await _mediator.Send(new GetTodoListsQuery()));
        }

        // GET: api/todolists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoListDto>> GetTodoList(long id)
        {
            var todoList = await _mediator.Send(new GetTodoListByIdQuery { Id = id });

            if (todoList == null)
            {
                return NotFound();
            }

            return Ok(todoList);
        }

        // PUT: api/todolists/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<TodoListDto>> PutTodoList(
            long id,
            UpdateTodoListRequest payload
        )
        {
            await _mediator.Send(new UpdateTodoListCommand { Id = id, Name = payload.Name });

            return Ok(await _mediator.Send(new GetTodoListByIdQuery { Id = id }));
        }

        // POST: api/todolists
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult> PostTodoList(CreateTodoListRequest payload)
        {
            var id = await _mediator.Send(new CreateTodoListCommand { Name = payload.Name });

            return CreatedAtAction(
                "GetTodoList",
                new { id },
                await _mediator.Send(new GetTodoListByIdQuery { Id = id })
            );
        }

        // DELETE: api/todolists/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoList(long id)
        {
            await _mediator.Send(new DeleteTodoListCommand { Id = id });

            return NoContent();
        }
    }
}
