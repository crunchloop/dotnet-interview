using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers;

[Route("api/todolists/{id:long}/todoitems")]
public class TodoTaskController : ControllerBase
{
    private readonly TodoContext _todoContext;

    public TodoTaskController(TodoContext todoContext)
    {
        this._todoContext = todoContext;
    }

    [HttpGet]
    public async Task<ActionResult<IList<TodoItem>>> GetTodoTasks([FromRoute] long id)
    {
        var todoItem = await _todoContext.TodoList.Include(todoList => todoList.Tasks)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (todoItem is null)
        {
            return NotFound();
        }

        return Ok(todoItem.Tasks);
    }

    [HttpPost]
    public async Task<ActionResult<TodoItem>> AddItem([FromRoute] long id, [FromBody] TodoItem item)
    {
        var todoList = await _todoContext.TodoList.Include(tl => tl.Tasks).FirstOrDefaultAsync(tl => tl.Id == id);

        if (todoList == null)
        {
            return NotFound();
        }

        await _todoContext.TodoItems.AddAsync(item);
        todoList.Tasks.Add(item);

        await _todoContext.SaveChangesAsync();
        return Ok(item);
    }
}