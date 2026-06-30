using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Todo.Application.Common.Exceptions;
using Todo.Application.TodoListItems.Commands.CreateTodoListItem;
using Todo.Application.TodoListItems.Commands.DeleteTodoListItem;
using Todo.Application.TodoListItems.Commands.UpdateTodoListItem;
using Todo.Domain.Entities;
using Todo.Persistence;

namespace Todo.Application.Tests.TodoListItems;

public class TodoListItemCommandHandlerTests
{
    private static readonly IPublisher Publisher = Mock.Of<IPublisher>();

    private static DbContextOptions<TodoContext> DatabaseContextOptions() =>
        new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoLists.Add(new TodoList { Id = 1, Name = "Task 1" });
        context.TodoListItems.Add(
            new TodoListItem
            {
                Id = 1,
                Name = "Item 1",
                TodoListId = 1,
            }
        );
        context.SaveChanges();
    }

    [Fact]
    public async Task CreateTodoListItem_WhenTodoListDoesntExist_ThrowsNotFoundException()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new CreateTodoListItemCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new CreateTodoListItemCommand { TodoListId = 2, Name = "Item 2" },
                    CancellationToken.None
                )
        );
    }

    [Fact]
    public async Task CreateTodoListItem_WhenCalled_CreatesTodoListItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new CreateTodoListItemCommandHandler(context);

        var id = await handler.Handle(
            new CreateTodoListItemCommand { TodoListId = 1, Name = "Item 2" },
            CancellationToken.None
        );

        Assert.Equal(2, context.TodoListItems.Count());
        Assert.Equal("Item 2", (await context.TodoListItems.FindAsync(id))?.Name);
    }

    [Fact]
    public async Task UpdateTodoListItem_WhenTodoListItemDoesntExist_ThrowsNotFoundException()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new UpdateTodoListItemCommandHandler(context, Publisher);

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new UpdateTodoListItemCommand
                    {
                        Id = 2,
                        TodoListId = 1,
                        Name = "Item 2",
                    },
                    CancellationToken.None
                )
        );
    }

    [Fact]
    public async Task UpdateTodoListItem_WhenCalled_UpdatesTheTodoListItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new UpdateTodoListItemCommandHandler(context, Publisher);

        await handler.Handle(
            new UpdateTodoListItemCommand
            {
                Id = 1,
                TodoListId = 1,
                Name = "Changed Item 1",
                IsComplete = true,
            },
            CancellationToken.None
        );

        var item = await context.TodoListItems.FindAsync(1L);
        Assert.Equal("Changed Item 1", item?.Name);
        Assert.True(item?.IsComplete);
    }

    [Fact]
    public async Task DeleteTodoListItem_WhenCalled_RemovesTodoListItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new DeleteTodoListItemCommandHandler(context, Publisher);

        await handler.Handle(
            new DeleteTodoListItemCommand { Id = 1, TodoListId = 1 },
            CancellationToken.None
        );

        Assert.Equal(0, context.TodoListItems.Count());
    }
}
