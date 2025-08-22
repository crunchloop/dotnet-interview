using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;
using TodoApi.Services.Interfaces;

namespace TodoApi.Tests.Controllers;

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
        var list = new TodoList { Id = 1, Name = "Test List" };
        context.TodoList.Add(list);

        context.TodoItems.Add(new TodoItem { Id = 1, Title = "Item 1", IsCompleted = false, TodoListId = 1 });
        context.TodoItems.Add(new TodoItem { Id = 2, Title = "Item 2", IsCompleted = true, TodoListId = 1 });

        context.SaveChanges();
    }

    [Fact]
    public async Task GetTodoItems_WhenCalled_ReturnsAllItemsForList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoItemService service = new TodoItemService(context);
        var controller = new TodoItemsController(service);

        var result = await controller.GetTodoItems(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IList<TodoItem>>(okResult.Value);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetTodoItem_WhenExists_ReturnsItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoItemService service = new TodoItemService(context);
        var controller = new TodoItemsController(service);

        var result = await controller.GetTodoItem(1, 1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var item = Assert.IsType<TodoItem>(okResult.Value);
        Assert.Equal(1, item.Id);
    }

    [Fact]
    public async Task GetTodoItem_WhenNotFound_ReturnsNotFound()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoItemService service = new TodoItemService(context);
        var controller = new TodoItemsController(service);

        var result = await controller.GetTodoItem(1, 99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PutTodoItem_WhenExists_UpdatesItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoItemService service = new TodoItemService(context);
        var controller = new TodoItemsController(service);

        var dto = new UpdateTodoItem { Title = "Updated Item", IsCompleted = true };
        var result = await controller.PutTodoItem(1, 1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updated = Assert.IsType<TodoItem>(okResult.Value);
        Assert.Equal("Updated Item", updated.Title);
        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task PutTodoItem_WhenNotFound_ReturnsNotFound()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoItemService service = new TodoItemService(context);
        var controller = new TodoItemsController(service);

        var result = await controller.PutTodoItem(1, 99, new UpdateTodoItem { Title = "Doesn't Exist", IsCompleted = false });

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostTodoItem_WhenValid_CreatesItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoItemService service = new TodoItemService(context);
        var controller = new TodoItemsController(service);

        var result = await controller.PostTodoItem(1, new CreateTodoItem { Title = "New Item" });

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        var created = Assert.IsType<TodoItem>(createdAt.Value);
        Assert.Equal("New Item", created.Title);
        Assert.Equal(3, context.TodoItems.Count());
    }

    [Fact]
    public async Task PostTodoItem_WhenListNotFound_ReturnsNotFound()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoItemService service = new TodoItemService(context);
        var controller = new TodoItemsController(service);

        var result = await controller.PostTodoItem(99, new CreateTodoItem { Title = "Invalid" });

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("List not found", notFound.Value);
    }

    [Fact]
    public async Task DeleteTodoItem_WhenExists_RemovesItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoItemService service = new TodoItemService(context);
        var controller = new TodoItemsController(service);

        var result = await controller.DeleteTodoItem(1, 2);

        Assert.IsType<NoContentResult>(result);
        Assert.Single(context.TodoItems); // only 1 left
    }

    [Fact]
    public async Task DeleteTodoItem_WhenNotFound_ReturnsNotFound()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        ITodoItemService service = new TodoItemService(context);
        var controller = new TodoItemsController(service);

        var result = await controller.DeleteTodoItem(1, 99);

        Assert.IsType<NotFoundResult>(result);
    }
}
