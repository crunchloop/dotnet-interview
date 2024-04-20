using TodoApi.Controllers;
using TodoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TodoApi.Tests.Controllers
{
    public class TodoItemsControllerTests
    {
        private DbContextOptions<TodoContext> DatabaseContextOptions()
        {
            return new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private void PopulateDatabaseContext(TodoContext context)
        {
            context.TodoList.Add(new Models.TodoList { Id = 1, Name = "Task 1" });
            context.TodoList.Add(new Models.TodoList { Id = 2, Name = "List 2", Items = new List<TodoItem>() { new TodoItem() { Id = 1, Title = "Item1", Description = "Desc", Completed = false } } });
            context.SaveChanges();
        }

        [Fact]
        public async Task GetTodoItem_WhenTodoListIdDoesntMatch_ReturnsNotFound() {

            using (var context = new TodoContext(DatabaseContextOptions()))
            {
                PopulateDatabaseContext(context);

                var controller = new TodoItemController(context);

                var result = await controller.GetTodoItem(1, 3);

                Assert.IsType<NotFound>(result);
            }
        }

        [Fact]
        public async Task GetTodoItem_WhenTodoItemIdDoesntMatch_ReturnsNotFound()
        {

            using (var context = new TodoContext(DatabaseContextOptions()))
            {
                PopulateDatabaseContext(context);

                var controller = new TodoItemController(context);

                var result = await controller.GetTodoItem(3, 2);

                Assert.IsType<NotFound>(result);
            }
        }

        [Fact]
        public async Task GetTodoItem_WhenTodoItemIdMatch_ReturnsOk()
        {

            using (var context = new TodoContext(DatabaseContextOptions()))
            {
                PopulateDatabaseContext(context);

                var controller = new TodoItemController(context);

                var result = await controller.GetTodoItem(1, 2);

                Assert.IsType<Ok>(result);
            }
        }


        [Fact]
        public async Task PostTodoItem_WhenTodoListIdDoesntMatch_ReturnsNotFound()
        {

            using (var context = new TodoContext(DatabaseContextOptions()))
            {
                PopulateDatabaseContext(context);

                var controller = new TodoItemController(context);

                var result = await controller.Post(new TodoItem() { Id = 2, Title = "prueba", Description = "prueba", Completed = true }, 4);

                Assert.IsType<NotFound>(result);
            }
        }


        [Fact]
        public async Task PostTodoItem_WhenTodoListIdMatch_ReturnsNotFound()
        {

            using (var context = new TodoContext(DatabaseContextOptions()))
            {
                PopulateDatabaseContext(context);

                var controller = new TodoItemController(context);

                var result = await controller.Post(new TodoItem() { Id = 2, Title = "prueba", Description = "prueba", Completed = true }, 1);

                Assert.IsType<Ok>(result);
            }
        }
    }
}
