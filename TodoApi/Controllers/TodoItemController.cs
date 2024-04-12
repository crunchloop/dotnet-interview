using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers
{
    //api/todoLists/{4}/todoItems
    [Route("api/todoLists/{itemListId}/todoItems")]
    public class TodoItemController : Controller
    {
        private readonly ITodoItemRepository _itemRepository;

        public TodoItemController(ITodoItemRepository itemRepository)
        {
            this._itemRepository = itemRepository;
        }


        [HttpGet()]

        public async Task<ActionResult<List<TodoItem>>> GetAllItemFromList(int itemListId)
        {
            var listOfItems = await this._itemRepository.GetAllItems(itemListId);
            return Json(listOfItems);
        }


        [HttpPost()]

        public async Task<ActionResult> CreateItem(int itemListId, TodoItem input)
        {
            input.TodoListId = itemListId;
            await this._itemRepository.CreateItem(input);
            return Ok();
        }
    }
}
