using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Services;

#nullable disable
public class TodoListServiceTests
{
    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void Seed(TodoContext context)
    {
        context.TodoList.Add(new TodoList { Id = 1, Name = "List 1" });
        context.TodoList.Add(new TodoList { Id = 2, Name = "List 2" });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllLists()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoListService(context);

        var lists = await service.GetAllAsync();

        Assert.Equal(2, lists.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoListService(context);

        var list = await service.GetByIdAsync(1);

        Assert.NotNull(list);
        Assert.Equal("List 1", list.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoListService(context);

        var list = await service.GetByIdAsync(99);

        Assert.Null(list);
    }

    [Fact]
    public async Task CreateAsync_AddsNewList()
    {
        using var context = new TodoContext(DatabaseContextOptions());

        var service = new TodoListService(context);

        var dto = new CreateTodoList { Name = "New List" };
        var created = await service.CreateAsync(dto);

        Assert.NotNull(created);
        Assert.Equal("New List", created.Name);
        Assert.Equal(1, context.TodoList.Count());
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoListService(context);

        var dto = new UpdateTodoList { Name = "Updated List" };
        var updated = await service.UpdateAsync(1, dto);

        Assert.NotNull(updated);
        Assert.Equal("Updated List", updated.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoListService(context);

        var dto = new UpdateTodoList { Name = "Non-existent" };
        var updated = await service.UpdateAsync(99, dto);

        Assert.Null(updated);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoListService(context);

        var result = await service.DeleteAsync(1);

        Assert.True(result);
        Assert.Single(context.TodoList); // only 1 left
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoListService(context);

        var result = await service.DeleteAsync(99);

        Assert.False(result);
        Assert.Equal(2, context.TodoList.Count());
    }

    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoListService(context);

        var exists = await service.ExistsAsync(1);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WhenNotFound_ReturnsFalse()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        Seed(context);

        var service = new TodoListService(context);

        var exists = await service.ExistsAsync(99);

        Assert.False(exists);
    }
}
