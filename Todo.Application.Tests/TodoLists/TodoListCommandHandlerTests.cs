using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Todo.Application.Common.Exceptions;
using Todo.Application.Sync.Notifications;
using Todo.Application.TodoLists.Commands.CreateTodoList;
using Todo.Application.TodoLists.Commands.DeleteTodoList;
using Todo.Application.TodoLists.Commands.UpdateTodoList;
using Todo.Domain.Entities;
using Todo.Persistence;

namespace Todo.Application.Tests.TodoLists;

public class TodoListCommandHandlerTests
{
    private static DbContextOptions<TodoContext> DatabaseContextOptions() =>
        new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoLists.Add(new TodoList { Id = 1, Name = "Task 1" });
        context.TodoLists.Add(
            new TodoList
            {
                Id = 2,
                Name = "Task 2",
                ExternalId = "ext-2",
            }
        );
        context.SaveChanges();
    }

    [Fact]
    public async Task CreateTodoList_WhenCalled_CreatesTodoList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new CreateTodoListCommandHandler(context, Mock.Of<IPublisher>());

        var id = await handler.Handle(
            new CreateTodoListCommand { Name = "Task 3" },
            CancellationToken.None
        );

        Assert.Equal(3, context.TodoLists.Count());
        Assert.Equal("Task 3", (await context.TodoLists.FindAsync(id))?.Name);
    }

    [Fact]
    public async Task CreateTodoList_WhenCalled_PublishesCreatedNotificationWithNewId()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new CreateTodoListCommandHandler(context, publisher.Object);

        var id = await handler.Handle(
            new CreateTodoListCommand { Name = "Task 3" },
            CancellationToken.None
        );

        publisher.Verify(
            p =>
                p.Publish(
                    It.Is<TodoListCreatedNotification>(n => n.TodoListId == id),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateTodoList_WhenTodoListDoesntExist_ThrowsNotFoundException()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new UpdateTodoListCommandHandler(context, publisher.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new UpdateTodoListCommand { Id = 99, Name = "Task 3" },
                    CancellationToken.None
                )
        );

        // A failed update must not emit an outbound notification.
        publisher.Verify(
            p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateTodoList_WhenCalled_UpdatesTheTodoListAndPublishesNotification()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new UpdateTodoListCommandHandler(context, publisher.Object);

        await handler.Handle(
            new UpdateTodoListCommand { Id = 2, Name = "Changed Task 2" },
            CancellationToken.None
        );

        Assert.Equal("Changed Task 2", (await context.TodoLists.FindAsync(2L))?.Name);
        publisher.Verify(
            p =>
                p.Publish(
                    It.Is<TodoListUpdatedNotification>(n => n.TodoListId == 2),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteTodoList_WhenTodoListDoesntExist_ThrowsNotFoundException()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new DeleteTodoListCommandHandler(context, publisher.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteTodoListCommand { Id = 99 }, CancellationToken.None)
        );

        publisher.Verify(
            p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task DeleteTodoList_WhenCalled_RemovesTodoList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new DeleteTodoListCommandHandler(context, Mock.Of<IPublisher>());

        await handler.Handle(new DeleteTodoListCommand { Id = 1 }, CancellationToken.None);

        Assert.Equal(1, context.TodoLists.Count());
        Assert.Null(await context.TodoLists.FindAsync(1L));
    }

    [Fact]
    public async Task DeleteTodoList_WhenSynced_PublishesDeletedNotificationWithCapturedExternalId()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new DeleteTodoListCommandHandler(context, publisher.Object);

        // List 2 was synced (ExternalId "ext-2"); the deletion must carry that id outbound.
        await handler.Handle(new DeleteTodoListCommand { Id = 2 }, CancellationToken.None);

        publisher.Verify(
            p =>
                p.Publish(
                    It.Is<TodoListDeletedNotification>(n => n.ExternalId == "ext-2"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteTodoList_WhenNeverSynced_PublishesDeletedNotificationWithNullExternalId()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new DeleteTodoListCommandHandler(context, publisher.Object);

        // List 1 was never synced, so the outbound notification carries a null external id.
        await handler.Handle(new DeleteTodoListCommand { Id = 1 }, CancellationToken.None);

        publisher.Verify(
            p =>
                p.Publish(
                    It.Is<TodoListDeletedNotification>(n => n.ExternalId == null),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
