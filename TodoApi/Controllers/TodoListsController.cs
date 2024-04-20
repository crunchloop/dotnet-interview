using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repository.Interfaces;

namespace TodoApi.Controllers
{
    [Route("api/todolists")]
    [ApiController]
    public class TodoListsController : ControllerBase
    {
        private readonly ITodoListRepository _todoListRepository;

        public TodoListsController(ITodoListRepository todoListRepositor)
        {
            _todoListRepository = todoListRepositor;
        }

        // GET: api/todolists
        [HttpGet]
        public async Task<ActionResult<IList<TodoList>>> GetTodoLists()
        {
            ICollection<TodoList>? todoList;

            try {
                todoList = await _todoListRepository.GetAll();
            } catch (NullReferenceException) {
                return NotFound();
            }

            return Ok(todoList);
        }

        // GET: api/todolists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoList>> GetTodoList(long id)
        {
            TodoList? list;

            try {
                list = await _todoListRepository.Get(id);
            }
            catch
            {
                return NotFound();
            }

            return Ok(list);
        }

        // PUT: api/todolists/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult> PutTodoList(long id, TodoList todoList)
        {

            try {
                await _todoListRepository.Update(id, todoList);
            }
            catch (FormatException)
            {
                return BadRequest();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/todolists
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoList>> PostTodoList(TodoList todoList)
        {
            long newId = -1;

            try {
                newId = await _todoListRepository.Create(todoList);
            }
            catch
            {
                return Problem("Entity set 'TodoContext.TodoList'  is null.");

            }

            return CreatedAtAction("GetTodoList", new { id = newId }, todoList);
        }

        // DELETE: api/todolists/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoList(long id)
        {
            try {
                await _todoListRepository.Delete(id);
            }
            catch {
                return NotFound();
            }

            return NoContent();
        }
    }
}
