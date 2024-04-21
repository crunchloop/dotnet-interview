using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repository.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoApi.Controllers
{
    [Route("api/todolist/{idList:long}/todoitem")]
    [ApiController]
    public class TodoItemController : ControllerBase
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public TodoItemController(ITodoItemRepository repository)
        {
            _todoItemRepository = repository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id, long idList)
        {
            TodoItem? item;

            try{
                item = await _todoItemRepository.Get(idList, id);
            }
            catch (KeyNotFoundException) {
                return NotFound();
            }

            return Ok(item);
        }

        // POST api/<TodoItemController>
        [HttpPost]
        public async Task<ActionResult<TodoItem>> Post([FromBody] TodoItem item, long idList)
        {
            long newId;

            try
            {
                newId = await _todoItemRepository.Create(idList, item);
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
            catch (Exception) {
                return StatusCode(500 ,"Database error");
            }

            return CreatedAtAction("GetTodoItem", new { id = item.Id, idList }, item);
        }

        // PUT api/<TodoItemController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, TodoItem item)
        {

            try
            {
                await _todoItemRepository.Update(id, item);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest();
            }
            catch (DbUpdateConcurrencyException e)
            {
                return StatusCode(500, $"Error updating the object: {e.Message}");
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }

            return Ok();
        }


        // DELETE api/<TodoItemController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id, long idList)
        {
            try
            {
                await _todoItemRepository.Delete(idList, id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error saving the change: {e.Message}");
            }

            return Ok();
        }

    }
}
