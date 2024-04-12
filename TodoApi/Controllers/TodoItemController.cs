using Microsoft.AspNetCore.Mvc;
using TodoApi.Application.Middlewares.Contracts;
using TodoApi.Models;
using TodoApi.Repositories;

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


        [HttpPost()]

        public async Task<ActionResult> CreateItem(int itemListId, TodoItem input)
        {
            input.TodoListId = itemListId;
            await this._todoItemMiddleware.CreateItem(input);
            return Ok();
        }
    }
}
