using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
  [Route("api/todolists")]
  [ApiController]
  public class TodoListsController : ControllerBase
  {
    private readonly TodoContext _context;

    public TodoListsController(TodoContext context)
    {
      _context = context;
    }

    // GET: api/todolists
    [HttpGet]
    public async Task<ActionResult<IList<TodoList>>> GetTodoLists()
    {
      if (_context.TodoList == null)
        return NotFound();

      return Ok(await _context.TodoList.ToListAsync());
    }

    // GET: api/todolists/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoList>> GetTodoList(long id)
    {
      if (_context.TodoList == null)
        return NotFound();

      var todoList = await _context.TodoList.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == id);

      if (todoList == null)
        return NotFound();

      return Ok(todoList);
    }

    // PUT: api/todolists/5
    // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<ActionResult> PutTodoList(long id, TodoList todoList)
    {
      if (id != todoList.Id)
        return BadRequest();

      _context.Entry(todoList).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!TodoListExists(id)) 
          return NotFound();
        else
          throw;
      }

      return NoContent();
    }

    // POST: api/todolists
    // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TodoList>> PostTodoList(TodoList todoList)
    {
      if (_context.TodoList == null) 
        return Problem("Entity set 'TodoContext.TodoList'  is null.");
      _context.TodoList.Add(todoList);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetTodoList", new { id = todoList.Id }, todoList);
    }

    // DELETE: api/todolists/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTodoList(long id)
    {
      if (_context.TodoList == null)
        return NotFound();
      var todoList = await _context.TodoList.FindAsync(id);
      if (todoList == null)
        return NotFound();

      _context.TodoList.Remove(todoList);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool TodoListExists(long id)
    {
      return (_context.TodoList?.Any(e => e.Id == id)).GetValueOrDefault();
    }
  }
}
