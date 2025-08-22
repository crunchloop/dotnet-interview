using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;
using TodoApi.Services.Interfaces;

namespace TodoApi.Tests.Controllers;

#nullable disable
public class TodoListsControllerTests
{
    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoList.Add(new TodoList { Id = 1, Name = "Task 1" });
        context.TodoList.Add(new TodoList { Id = 2, Name = "Task 2" });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetTodoLists_WhenCalled_ReturnsAllLists()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoListService service = new TodoListService(context);
        var controller = new TodoListsController(service);

        var result = await controller.GetTodoLists();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var lists = Assert.IsAssignableFrom<IList<TodoList>>(okResult.Value);
        Assert.Equal(2, lists.Count);
    }

    [Fact]
    public async Task GetTodoList_WhenExists_ReturnsList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoListService service = new TodoListService(context);
        var controller = new TodoListsController(service);

        var result = await controller.GetTodoList(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<TodoList>(okResult.Value);
        Assert.Equal(1, list.Id);
    }

    [Fact]
    public async Task GetTodoList_WhenNotFound_ReturnsNotFound()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoListService service = new TodoListService(context);
        var controller = new TodoListsController(service);

        var result = await controller.GetTodoList(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PutTodoList_WhenDoesNotExist_ReturnsNotFound()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoListService service = new TodoListService(context);
        var controller = new TodoListsController(service);

        var result = await controller.PutTodoList(
            99,
            new UpdateTodoList { Name = "Doesn't Exist" }
        );

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PutTodoList_WhenExists_UpdatesList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoListService service = new TodoListService(context);
        var controller = new TodoListsController(service);

        var result = await controller.PutTodoList(
            2,
            new UpdateTodoList { Name = "Changed Task 2" }
        );

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updated = Assert.IsType<TodoList>(okResult.Value);
        Assert.Equal("Changed Task 2", updated.Name);
    }

    [Fact]
    public async Task PostTodoList_WhenValid_CreatesList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoListService service = new TodoListService(context);
        var controller = new TodoListsController(service);

        var result = await controller.PostTodoList(new CreateTodoList { Name = "Task 3" });

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        var created = Assert.IsType<TodoList>(createdAt.Value);
        Assert.Equal("Task 3", created.Name);
        Assert.Equal(3, context.TodoList.Count());
    }

    [Fact]
    public async Task DeleteTodoList_WhenExists_RemovesList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoListService service = new TodoListService(context);
        var controller = new TodoListsController(service);

        var result = await controller.DeleteTodoList(2);

        Assert.IsType<NoContentResult>(result);
        Assert.Single(context.TodoList); // only 1 left
    }

    [Fact]
    public async Task DeleteTodoList_WhenNotFound_ReturnsNotFound()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoListService service = new TodoListService(context);
        var controller = new TodoListsController(service);

        var result = await controller.DeleteTodoList(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
