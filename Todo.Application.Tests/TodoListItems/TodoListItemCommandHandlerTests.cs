using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Todo.Application.Common.Exceptions;
using Todo.Application.Sync.Notifications;
using Todo.Application.TodoListItems.Commands.CreateTodoListItem;
using Todo.Application.TodoListItems.Commands.DeleteTodoListItem;
using Todo.Application.TodoListItems.Commands.UpdateTodoListItem;
using Todo.Domain.Entities;
using Todo.Persistence;

namespace Todo.Application.Tests.TodoListItems;

public class TodoListItemCommandHandlerTests
{
    private static DbContextOptions<TodoContext> DatabaseContextOptions() =>
        new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoLists.Add(
            new TodoList
            {
                Id = 1,
                Name = "Task 1",
                ExternalId = "ext-list-1",
            }
        );
        // A second list lets us assert that the (Id, TodoListId) pair is matched, not Id alone.
        context.TodoLists.Add(new TodoList { Id = 2, Name = "Task 2" });
        context.TodoListItems.Add(
            new TodoListItem
            {
                Id = 1,
                Name = "Item 1",
                TodoListId = 1,
                ExternalId = "ext-item-1",
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
                    new CreateTodoListItemCommand { TodoListId = 99, Name = "Item 2" },
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
        var created = await context.TodoListItems.FindAsync(id);
        Assert.Equal("Item 2", created?.Name);
        Assert.False(created?.IsComplete); // defaults to incomplete
        Assert.Equal(1, created?.TodoListId);
    }

    [Fact]
    public async Task CreateTodoListItem_WhenIsCompleteRequested_PersistsCompletedItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new CreateTodoListItemCommandHandler(context);

        var id = await handler.Handle(
            new CreateTodoListItemCommand
            {
                TodoListId = 1,
                Name = "Done Item",
                IsComplete = true,
            },
            CancellationToken.None
        );

        Assert.True((await context.TodoListItems.FindAsync(id))?.IsComplete);
    }

    [Fact]
    public async Task UpdateTodoListItem_WhenTodoListItemDoesntExist_ThrowsNotFoundException()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new UpdateTodoListItemCommandHandler(context, publisher.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new UpdateTodoListItemCommand
                    {
                        Id = 99,
                        TodoListId = 1,
                        Name = "Item 2",
                    },
                    CancellationToken.None
                )
        );

        publisher.Verify(
            p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateTodoListItem_WhenItemBelongsToAnotherList_ThrowsNotFoundException()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new UpdateTodoListItemCommandHandler(context, Mock.Of<IPublisher>());

        // Item 1 exists, but under list 1 — addressing it via list 2 must not match.
        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new UpdateTodoListItemCommand
                    {
                        Id = 1,
                        TodoListId = 2,
                        Name = "Item 2",
                    },
                    CancellationToken.None
                )
        );
    }

    [Fact]
    public async Task UpdateTodoListItem_WhenCalled_UpdatesTheTodoListItemAndPublishesNotification()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new UpdateTodoListItemCommandHandler(context, publisher.Object);

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

        publisher.Verify(
            p =>
                p.Publish(
                    It.Is<TodoListItemUpdatedNotification>(n =>
                        n.TodoListId == 1 && n.TodoListItemId == 1
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteTodoListItem_WhenTodoListItemDoesntExist_ThrowsNotFoundException()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new DeleteTodoListItemCommandHandler(context, publisher.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new DeleteTodoListItemCommand { Id = 99, TodoListId = 1 },
                    CancellationToken.None
                )
        );

        publisher.Verify(
            p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task DeleteTodoListItem_WhenItemBelongsToAnotherList_ThrowsNotFoundException()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new DeleteTodoListItemCommandHandler(context, Mock.Of<IPublisher>());

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new DeleteTodoListItemCommand { Id = 1, TodoListId = 2 },
                    CancellationToken.None
                )
        );

        Assert.Equal(1, context.TodoListItems.Count());
    }

    [Fact]
    public async Task DeleteTodoListItem_WhenCalled_RemovesItemAndPublishesNotificationWithExternalIds()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var publisher = new Mock<IPublisher>();
        var handler = new DeleteTodoListItemCommandHandler(context, publisher.Object);

        await handler.Handle(
            new DeleteTodoListItemCommand { Id = 1, TodoListId = 1 },
            CancellationToken.None
        );

        Assert.Equal(0, context.TodoListItems.Count());

        // External ids of both the list and the item are captured before removal so the
        // outbound handler can address the nested remote resource.
        publisher.Verify(
            p =>
                p.Publish(
                    It.Is<TodoListItemDeletedNotification>(n =>
                        n.ListExternalId == "ext-list-1" && n.ItemExternalId == "ext-item-1"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
