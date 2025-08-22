using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Services;

#nullable disable
public class TodoItemServiceTests
{
    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void Seed(TodoContext context)
    {
        var list = new TodoList { Id = 1, Name = "Test List" };
        context.TodoList.Add(list);

        context.TodoItems.Add(new TodoItem { Id = 1, Title = "Item 1", IsCompleted = false, TodoListId = 1 });
        context.TodoItems.Add(new TodoItem { Id = 2, Title = "Item 2", IsCompleted = true, TodoListId = 1 });

        context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsItemsForList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoItemService(context);

        var items = await service.GetAllAsync(1);

        Assert.Equal(2, items.Count);
        Assert.All(items, i => Assert.Equal(1, i.TodoListId));
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoItemService(context);

        var item = await service.GetByIdAsync(1, 1);

        Assert.NotNull(item);
        Assert.Equal("Item 1", item.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoItemService(context);

        var item = await service.GetByIdAsync(1, 99);

        Assert.Null(item);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoItemService(context);

        var dto = new UpdateTodoItem { Title = "Updated", IsCompleted = true };
        var updated = await service.UpdateAsync(1, 1, dto);

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Title);
        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoItemService(context);

        var dto = new UpdateTodoItem { Title = "Doesn't Exist", IsCompleted = false };
        var updated = await service.UpdateAsync(1, 99, dto);

        Assert.Null(updated);
    }

    [Fact]
    public async Task CreateAsync_WhenListExists_CreatesItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoItemService(context);

        var dto = new CreateTodoItem { Title = "New Item" };
        var created = await service.CreateAsync(1, dto);

        Assert.NotNull(created);
        Assert.Equal("New Item", created.Title);
        Assert.Equal(3, context.TodoItems.Count());
    }

    [Fact]
    public async Task CreateAsync_WhenListNotFound_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoItemService(context);

        var dto = new CreateTodoItem { Title = "Invalid Item" };
        var created = await service.CreateAsync(99, dto);

        Assert.Null(created);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoItemService(context);

        var result = await service.DeleteAsync(1, 2);

        Assert.True(result);
        Assert.Single(context.TodoItems);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoItemService(context);

        var result = await service.DeleteAsync(1, 99);

        Assert.False(result);
        Assert.Equal(2, context.TodoItems.Count());
    }
}
