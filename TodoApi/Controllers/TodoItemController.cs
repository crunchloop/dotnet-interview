using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Application.Middlewares.Contracts;
using TodoApi.Application.ViewModels;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    //api/todoLists/{4}/todoItems
    [Route("api/todoLists/{itemListId}/todoItems")]
    public class TodoItemController : Controller
    {
        private readonly ITodoItemMiddleware _todoItemMiddleware;

        public TodoItemController(ITodoItemMiddleware todoItemMiddleware)
        {
            this._todoItemMiddleware = todoItemMiddleware;
        }


        [HttpGet()]

        public async Task<ActionResult<List<TodoItem>>> GetAllItemFromList(int itemListId)
        {
            var listOfItems = await this._todoItemMiddleware.GetAllItems(itemListId);
            return Json(listOfItems);
        }


        [HttpPost]

        public async Task<ActionResult> CreateItem(int itemListId, [FromBody] CreateTodoItemViewModel input)
        {
            input.TodoListId = itemListId;
            var itemId = await this._todoItemMiddleware.CreateItem(input);
            return Ok(itemId);
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateItem(int id, [FromBody] UpdateTodoItemViewModel input)
        {
            await this._todoItemMiddleware.UpdateItem(id, input);
            return Ok();
        }


        [HttpDelete("{itemId}")]

        public async Task<ActionResult> DeleteItem(int itemId)
        {
            await this._todoItemMiddleware.DeleteItem(itemId);
            return Ok();
        }
    }
}
