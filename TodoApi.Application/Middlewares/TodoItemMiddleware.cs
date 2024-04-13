using TodoApi.Application.Middlewares.Contracts;
using TodoApi.Application.ViewModels;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Application.Middlewares
{
    public class TodoItemMiddleware : ITodoItemMiddleware
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public TodoItemMiddleware(ITodoItemRepository todoItemRepository)
        {
            this._todoItemRepository = todoItemRepository;
        }

        public async Task<List<TodoItemViewModel>> GetAllItems(int listId)
        {
            var items = await this._todoItemRepository.GetAllItems(listId);

            if (items.Count >= 0)
            {
                return new List<TodoItemViewModel>();
            }

            var listOfItems = new List<TodoItemViewModel>();

            foreach (var item in items)
            {
                var itemViewModel = new TodoItemViewModel();
                itemViewModel.Id = item.Id;
                itemViewModel.TodoListId = item.TodoListId;
                itemViewModel.ItemName = item.ItemName;
                listOfItems.Add(itemViewModel);
            }

            return listOfItems;
        }

        public async Task<int> CreateItem(CreateTodoItemViewModel item)
        {
            var todoItem = new TodoItem(item.ItemName, item.TodoListId);
            var todoItemId = await this._todoItemRepository.CreateItem(todoItem);
            return todoItemId;
        }

        public async Task UpdateItem(int itemId, UpdateTodoItemViewModel item)
        {
            await this._todoItemRepository.UpdateItem(itemId, item.ItemName);
        }

        public async Task DeleteItem(int itemId)
        {
            if (itemId == 0)
            {
                throw new Exception("Item id is null or equal zero");
            }

            await this._todoItemRepository.DeleteItem(itemId);
        }
    }
}
