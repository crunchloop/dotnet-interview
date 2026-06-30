using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Todo.Application.Common.Exceptions;
using Todo.Application.TodoLists.Commands.CreateTodoList;
using Todo.Application.TodoLists.Commands.DeleteTodoList;
using Todo.Application.TodoLists.Commands.UpdateTodoList;
using Todo.Domain.Entities;
using Todo.Persistence;

namespace Todo.Application.Tests.TodoLists;

public class TodoListCommandHandlerTests
{
    private static readonly IPublisher Publisher = Mock.Of<IPublisher>();

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
    public async Task CreateTodoList_WhenCalled_CreatesTodoList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new CreateTodoListCommandHandler(context, Publisher);

        var id = await handler.Handle(
            new CreateTodoListCommand { Name = "Task 3" },
            CancellationToken.None
        );

        Assert.Equal(3, context.TodoLists.Count());
        Assert.Equal("Task 3", (await context.TodoLists.FindAsync(id))?.Name);
    }

    [Fact]
    public async Task UpdateTodoList_WhenTodoListDoesntExist_ThrowsNotFoundException()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new UpdateTodoListCommandHandler(context, Publisher);

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new UpdateTodoListCommand { Id = 3, Name = "Task 3" },
                    CancellationToken.None
                )
        );
    }

    [Fact]
    public async Task UpdateTodoList_WhenCalled_UpdatesTheTodoList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new UpdateTodoListCommandHandler(context, Publisher);

        await handler.Handle(
            new UpdateTodoListCommand { Id = 2, Name = "Changed Task 2" },
            CancellationToken.None
        );

        Assert.Equal("Changed Task 2", (await context.TodoLists.FindAsync(2L))?.Name);
    }

    [Fact]
    public async Task DeleteTodoList_WhenCalled_RemovesTodoList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new DeleteTodoListCommandHandler(context, Publisher);

        await handler.Handle(new DeleteTodoListCommand { Id = 2 }, CancellationToken.None);

        Assert.Equal(1, context.TodoLists.Count());
    }
}
