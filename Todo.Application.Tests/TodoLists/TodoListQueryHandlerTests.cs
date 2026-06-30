using Microsoft.EntityFrameworkCore;
using Todo.Application.TodoLists.Queries.GetTodoListById;
using Todo.Application.TodoLists.Queries.GetTodoLists;
using Todo.Domain.Entities;
using Todo.Persistence;

namespace Todo.Application.Tests.TodoLists;

public class TodoListQueryHandlerTests
{
    private static DbContextOptions<TodoContext> DatabaseContextOptions() =>
        new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoLists.Add(new TodoList { Id = 1, Name = "Task 1" });
        context.TodoLists.Add(new TodoList { Id = 2, Name = "Task 2" });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetTodoLists_WhenCalled_ReturnsTodoListList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListsQueryHandler(context);

        var result = await handler.Handle(new GetTodoListsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetTodoLists_WhenNoListsExist_ReturnsEmptyList()
    {
        using var context = new TodoContext(DatabaseContextOptions());

        var handler = new GetTodoListsQueryHandler(context);

        var result = await handler.Handle(new GetTodoListsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTodoListById_WhenCalled_ReturnsTodoListById()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetTodoListByIdQuery { Id = 1 },
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task GetTodoListById_WhenTodoListDoesntExist_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetTodoListByIdQuery { Id = 3 },
            CancellationToken.None
        );

        Assert.Null(result);
    }
}
