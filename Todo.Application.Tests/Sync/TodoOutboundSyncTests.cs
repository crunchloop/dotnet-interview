using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Todo.Application.Common.Interfaces;
using Todo.Application.Integration.ExternalApi.Dtos;
using Todo.Application.Sync.Notifications;
using Todo.Application.Sync.Outbound;
using Todo.Domain.Entities;
using Todo.Persistence;

namespace Todo.Application.Tests.Sync;

public class TodoOutboundSyncTests
{
    private static TodoContext NewContext() =>
        new(
            new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options
        );

    [Fact]
    public async Task ListCreated_PushesToExternal_AndPersistsExternalIds()
    {
        using var context = NewContext();
        context.TodoLists.Add(
            new TodoList
            {
                Id = 1,
                Name = "New List",
                Items =
                {
                    new TodoListItem
                    {
                        Id = 10,
                        Name = "Item",
                        IsComplete = false,
                    },
                },
            }
        );
        await context.SaveChangesAsync();

        var client = new Mock<IExternalTodoApiClient>();
        client
            .Setup(c =>
                c.CreateTodoListAsync(It.IsAny<CreateTodoListBody>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new ExternalTodoList
                {
                    Id = "ext-1",
                    SourceId = "1",
                    Name = "New List",
                    Items =
                    {
                        new ExternalTodoItem { Id = "ext-item-10", SourceId = "10" },
                    },
                }
            );

        var handler = new TodoListCreatedNotificationHandler(
            context,
            client.Object,
            NullLogger<TodoListCreatedNotificationHandler>.Instance
        );

        await handler.Handle(new TodoListCreatedNotification(1), CancellationToken.None);

        var list = await context.TodoLists.Include(l => l.Items).SingleAsync();
        Assert.Equal("ext-1", list.ExternalId);
        Assert.Equal("ext-item-10", Assert.Single(list.Items).ExternalId);
    }

    [Fact]
    public async Task ListUpdated_CallsExternal_OnlyWhenSynced()
    {
        using var context = NewContext();
        context.TodoLists.Add(
            new TodoList
            {
                Id = 1,
                Name = "Synced",
                ExternalId = "ext-1",
            }
        );
        context.TodoLists.Add(new TodoList { Id = 2, Name = "Unsynced" });
        await context.SaveChangesAsync();

        var client = new Mock<IExternalTodoApiClient>();
        client
            .Setup(c =>
                c.UpdateTodoListAsync(
                    It.IsAny<string>(),
                    It.IsAny<UpdateTodoListBody>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new ExternalTodoList { Id = "ext-1" });

        var handler = new TodoListUpdatedNotificationHandler(
            context,
            client.Object,
            NullLogger<TodoListUpdatedNotificationHandler>.Instance
        );

        await handler.Handle(new TodoListUpdatedNotification(1), CancellationToken.None);
        await handler.Handle(new TodoListUpdatedNotification(2), CancellationToken.None);

        client.Verify(
            c =>
                c.UpdateTodoListAsync(
                    "ext-1",
                    It.IsAny<UpdateTodoListBody>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        client.Verify(
            c =>
                c.UpdateTodoListAsync(
                    It.IsAny<string>(),
                    It.IsAny<UpdateTodoListBody>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ListDeleted_CallsExternal_OnlyWhenExternalIdPresent()
    {
        var client = new Mock<IExternalTodoApiClient>();
        var handler = new TodoListDeletedNotificationHandler(
            client.Object,
            NullLogger<TodoListDeletedNotificationHandler>.Instance
        );

        await handler.Handle(new TodoListDeletedNotification("ext-1"), CancellationToken.None);
        await handler.Handle(new TodoListDeletedNotification(null), CancellationToken.None);

        client.Verify(
            c => c.DeleteTodoListAsync("ext-1", It.IsAny<CancellationToken>()),
            Times.Once
        );
        client.Verify(
            c => c.DeleteTodoListAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ItemUpdated_CallsExternal_OnlyWhenListAndItemSynced()
    {
        using var context = NewContext();
        context.TodoLists.Add(
            new TodoList
            {
                Id = 1,
                Name = "List",
                ExternalId = "ext-list",
                Items =
                {
                    new TodoListItem
                    {
                        Id = 10,
                        Name = "Synced Item",
                        ExternalId = "ext-item",
                    },
                    new TodoListItem
                    {
                        Id = 11,
                        Name = "Unsynced Item",
                        ExternalId = null,
                    },
                },
            }
        );
        await context.SaveChangesAsync();

        var client = new Mock<IExternalTodoApiClient>();
        client
            .Setup(c =>
                c.UpdateTodoItemAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<UpdateTodoItemBody>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new ExternalTodoItem { Id = "ext-item" });

        var handler = new TodoListItemUpdatedNotificationHandler(
            context,
            client.Object,
            NullLogger<TodoListItemUpdatedNotificationHandler>.Instance
        );

        await handler.Handle(new TodoListItemUpdatedNotification(1, 10), CancellationToken.None);
        await handler.Handle(new TodoListItemUpdatedNotification(1, 11), CancellationToken.None);

        client.Verify(
            c =>
                c.UpdateTodoItemAsync(
                    "ext-list",
                    "ext-item",
                    It.IsAny<UpdateTodoItemBody>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        client.Verify(
            c =>
                c.UpdateTodoItemAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<UpdateTodoItemBody>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ItemDeleted_CallsExternal_OnlyWhenBothIdsPresent()
    {
        var client = new Mock<IExternalTodoApiClient>();
        var handler = new TodoListItemDeletedNotificationHandler(
            client.Object,
            NullLogger<TodoListItemDeletedNotificationHandler>.Instance
        );

        await handler.Handle(
            new TodoListItemDeletedNotification("ext-list", "ext-item"),
            CancellationToken.None
        );
        await handler.Handle(
            new TodoListItemDeletedNotification("ext-list", null),
            CancellationToken.None
        );

        client.Verify(
            c => c.DeleteTodoItemAsync("ext-list", "ext-item", It.IsAny<CancellationToken>()),
            Times.Once
        );
        client.Verify(
            c =>
                c.DeleteTodoItemAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
