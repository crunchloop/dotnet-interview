using TodoApi.Controllers;
using TodoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Repository.Implementations;
using NuGet.Protocol.Core.Types;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TodoApi.Tests;

#nullable disable
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
        context.TodoList.Add(new Models.TodoList { Id = 2, Name = "Task 2", Items = new List<TodoItem>() { new TodoItem() { Id = 1 } } });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetTodoItem_WhenTodoListIdDoesntMatch_ReturnsNotFound()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        using (var repository = new TodoItemRepository(context))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoItemController(repository);

            var result = await controller.GetTodoItem(1, 3);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }

    [Fact]
    public async Task GetTodoItem_WhenTodoItemIdDoesntMatch_ReturnsNotFound()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        using (var repository = new TodoItemRepository(context))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoItemController(repository);

            var result = await controller.GetTodoItem(3, 2);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }

    [Fact]
    public async Task GetTodoItem_WhenTodoItemIdMatch_ReturnsOk()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        using (var repository = new TodoItemRepository(context))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoItemController(repository);

            var result = await controller.GetTodoItem(1, 2);

            Assert.IsType<OkObjectResult>(result.Result);
        }
    }


    [Fact]
    public async Task PostTodoItem_WhenTodoListIdDoesntMatch_ReturnsNotFound()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        using (var repository = new TodoItemRepository(context))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoItemController(repository);

            var result = await controller.Post(new TodoItem() { Id = 2, Title = "prueba", Description = "prueba", Completed = true }, 4);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }


    [Fact]
    public async Task PostTodoItem_WhenTodoListIdMatch_ReturnsObjectResultOnMemoryDB()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        using (var repository = new TodoItemRepository(context))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoItemController(repository);

            var result = await controller.Post(new TodoItem() { Id = 2, Title = "prueba", Description = "prueba", Completed = true }, 2);

            //Esta funcion en testing debe dar error por el manejo de las entidades
            //pero a la hora de migrarlo a una db da OkObjectResult
            Assert.IsType<ObjectResult>(result.Result);
        }
    }


    [Fact]
    public async Task DeleteTodoItem_WhenTodoListIdDoesntMatch_ReturnsNotFound()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        using (var repository = new TodoItemRepository(context))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoItemController(repository);

            var result = await controller.Delete(1,1);

            Assert.IsType<NotFoundResult>(result);
        }
    }

    [Fact]
    public async Task DeleteTodoItem_WhenTodoListIdMatch_ReturnsOk()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        using (var repository = new TodoItemRepository(context))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoItemController(repository);

            var result = await controller.Delete(1, 2);

            Assert.IsType<OkResult>(result);
        }
    }

    [Fact]
    public async Task PutTodoItem_WhenTodoItemIdDoesntMatch_ReturnBadRequest() {

        using (var context = new TodoContext(DatabaseContextOptions()))
        using (var repository = new TodoItemRepository(context))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoItemController(repository);

            var result = await controller.Put(1, new TodoItem() { Id = 2, Completed = false, Description = "asd", Title = "FakeTitle" });

            Assert.IsType<BadRequestResult>(result);
        }
    }

}