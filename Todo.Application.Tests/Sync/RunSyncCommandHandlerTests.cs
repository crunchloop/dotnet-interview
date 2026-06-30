using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Todo.Application.Common.Interfaces;
using Todo.Application.Integration.ExternalApi.Dtos;
using Todo.Application.Sync.Commands.RunSync;
using Todo.Domain.Entities;
using Todo.Persistence;

namespace Todo.Application.Tests.Sync;

public class RunSyncCommandHandlerTests
{
    private static DbContextOptions<TodoContext> DatabaseContextOptions() =>
        new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static RunSyncCommandHandler CreateHandler(
        TodoContext context,
        IExternalTodoApiClient client
    ) => new(context, client, NullLogger<RunSyncCommandHandler>.Instance);

    private static Mock<IExternalTodoApiClient> ClientReturning(params ExternalTodoList[] remote)
    {
        var mock = new Mock<IExternalTodoApiClient>();
        mock.Setup(c => c.GetTodoListsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(remote);
        return mock;
    }

    [Fact]
    public async Task Inbound_CreatesLocalList_ForRemoteOriginList()
    {
        using var context = new TodoContext(DatabaseContextOptions());

        var remote = new ExternalTodoList
        {
            Id = "ext-1",
            SourceId = null, // originated remotely, no local counterpart
            Name = "Remote List",
            UpdatedAt = DateTime.UtcNow,
            Items =
            {
                new ExternalTodoItem
                {
                    Id = "ext-item-1",
                    Description = "Remote Item",
                    Completed = false,
                    UpdatedAt = DateTime.UtcNow,
                },
            },
        };

        var result = await CreateHandler(context, ClientReturning(remote).Object)
            .Handle(new RunSyncCommand(), CancellationToken.None);

        var list = await context.TodoLists.Include(l => l.Items).SingleAsync();
        Assert.Equal("Remote List", list.Name);
        Assert.Equal("ext-1", list.ExternalId);
        Assert.Equal("ext-item-1", Assert.Single(list.Items).ExternalId);

        Assert.Equal(1, result.ListsCreated);
        Assert.Equal(1, result.ItemsCreated);
    }

    [Fact]
    public async Task Inbound_UpdatesLocalList_WhenRemoteIsNewer()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        context.TodoLists.Add(
            new TodoList
            {
                Id = 1,
                Name = "Old Name",
                ExternalId = "ext-1",
            }
        );
        await context.SaveChangesAsync();

        var remote = new ExternalTodoList
        {
            Id = "ext-1",
            SourceId = "1",
            Name = "New Name",
            UpdatedAt = DateTime.UtcNow.AddDays(1), // clearly newer than the stamped local row
        };

        var result = await CreateHandler(context, ClientReturning(remote).Object)
            .Handle(new RunSyncCommand(), CancellationToken.None);

        Assert.Equal("New Name", (await context.TodoLists.FindAsync(1L))!.Name);
        Assert.Equal(1, result.ListsUpdated);
    }

    [Fact]
    public async Task Inbound_DoesNotUpdate_WhenRemoteIsNotNewer()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        context.TodoLists.Add(
            new TodoList
            {
                Id = 1,
                Name = "Local Name",
                ExternalId = "ext-1",
            }
        );
        await context.SaveChangesAsync();

        var remote = new ExternalTodoList
        {
            Id = "ext-1",
            SourceId = "1",
            Name = "Stale Remote Name",
            UpdatedAt = DateTime.UtcNow.AddDays(-1), // older than the local row
        };

        var result = await CreateHandler(context, ClientReturning(remote).Object)
            .Handle(new RunSyncCommand(), CancellationToken.None);

        Assert.Equal("Local Name", (await context.TodoLists.FindAsync(1L))!.Name);
        Assert.Equal(0, result.ListsUpdated);
    }

    [Fact]
    public async Task Inbound_DeletesLocalList_WhenSyncedListMissingRemotely()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        context.TodoLists.Add(
            new TodoList
            {
                Id = 1,
                Name = "Was Synced",
                ExternalId = "ext-gone",
            }
        );
        await context.SaveChangesAsync();

        // Remote returns nothing, so the previously synced list is treated as deleted.
        var result = await CreateHandler(context, ClientReturning().Object)
            .Handle(new RunSyncCommand(), CancellationToken.None);

        Assert.Empty(context.TodoLists);
        Assert.Equal(1, result.ListsDeleted);
    }

    [Fact]
    public async Task Inbound_ReconcilesItems_CreateUpdateDelete()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        context.TodoLists.Add(
            new TodoList
            {
                Id = 1,
                Name = "List",
                ExternalId = "ext-1",
                Items =
                {
                    new TodoListItem
                    {
                        Id = 10,
                        Name = "Keep/Update",
                        IsComplete = false,
                        ExternalId = "ext-item-10",
                    },
                    new TodoListItem
                    {
                        Id = 11,
                        Name = "Delete Me",
                        IsComplete = false,
                        ExternalId = "ext-item-11",
                    },
                },
            }
        );
        await context.SaveChangesAsync();

        var remote = new ExternalTodoList
        {
            Id = "ext-1",
            SourceId = "1",
            Name = "List",
            UpdatedAt = DateTime.UtcNow.AddDays(-1), // list itself unchanged
            Items =
            {
                // existing item, newer + changed -> update
                new ExternalTodoItem
                {
                    Id = "ext-item-10",
                    SourceId = "10",
                    Description = "Updated Desc",
                    Completed = true,
                    UpdatedAt = DateTime.UtcNow.AddDays(1),
                },
                // brand new remote item -> create locally
                new ExternalTodoItem
                {
                    Id = "ext-item-99",
                    SourceId = null,
                    Description = "New Remote Item",
                    Completed = false,
                    UpdatedAt = DateTime.UtcNow,
                },
                // note: ext-item-11 is absent -> delete locally
            },
        };

        var result = await CreateHandler(context, ClientReturning(remote).Object)
            .Handle(new RunSyncCommand(), CancellationToken.None);

        var items = await context
            .TodoListItems.Where(i => i.TodoListId == 1)
            .OrderBy(i => i.ExternalId)
            .ToListAsync();

        Assert.Equal(2, items.Count); // 10 (updated) + 99 (created); 11 deleted
        var updated = items.Single(i => i.ExternalId == "ext-item-10");
        Assert.Equal("Updated Desc", updated.Name);
        Assert.True(updated.IsComplete);
        Assert.Contains(items, i => i.ExternalId == "ext-item-99");
        Assert.DoesNotContain(items, i => i.ExternalId == "ext-item-11");

        Assert.Equal(1, result.ItemsUpdated);
        Assert.Equal(1, result.ItemsCreated);
        Assert.Equal(1, result.ItemsDeleted);
    }

    [Fact]
    public async Task Outbound_MirrorsNeverSyncedLocalList_AndPersistsExternalIds()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        context.TodoLists.Add(
            new TodoList
            {
                Id = 5,
                Name = "Local Only",
                Items =
                {
                    new TodoListItem
                    {
                        Id = 50,
                        Name = "Local Item",
                        IsComplete = false,
                    },
                },
            }
        );
        await context.SaveChangesAsync();

        var client = ClientReturning(); // remote knows nothing
        client
            .Setup(c =>
                c.CreateTodoListAsync(It.IsAny<CreateTodoListBody>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new ExternalTodoList
                {
                    Id = "ext-new",
                    SourceId = "5",
                    Name = "Local Only",
                    Items =
                    {
                        new ExternalTodoItem
                        {
                            Id = "ext-new-item",
                            SourceId = "50",
                            Description = "Local Item",
                        },
                    },
                }
            );

        await CreateHandler(context, client.Object)
            .Handle(new RunSyncCommand(), CancellationToken.None);

        var list = await context.TodoLists.Include(l => l.Items).SingleAsync();
        Assert.Equal("ext-new", list.ExternalId);
        Assert.Equal("ext-new-item", Assert.Single(list.Items).ExternalId);

        client.Verify(
            c =>
                c.CreateTodoListAsync(
                    It.IsAny<CreateTodoListBody>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Outbound_RecreatesList_WhenSyncedListHasNewLocalItems()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        context.TodoLists.Add(
            new TodoList
            {
                Id = 1,
                Name = "List",
                ExternalId = "ext-1",
                Items =
                {
                    new TodoListItem
                    {
                        Id = 10,
                        Name = "Synced",
                        ExternalId = "ext-item-10",
                    },
                    new TodoListItem
                    {
                        Id = 11,
                        Name = "Brand New Local",
                        ExternalId = null, // never synced -> triggers delete+recreate
                    },
                },
            }
        );
        await context.SaveChangesAsync();

        var remote = new ExternalTodoList
        {
            Id = "ext-1",
            SourceId = "1",
            Name = "List",
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Items =
            {
                new ExternalTodoItem
                {
                    Id = "ext-item-10",
                    SourceId = "10",
                    Description = "Synced",
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                },
            },
        };

        var client = ClientReturning(remote);
        client
            .Setup(c => c.DeleteTodoListAsync("ext-1", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        client
            .Setup(c =>
                c.CreateTodoListAsync(It.IsAny<CreateTodoListBody>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new ExternalTodoList
                {
                    Id = "ext-1b",
                    SourceId = "1",
                    Name = "List",
                    Items =
                    {
                        new ExternalTodoItem { Id = "ext-item-10b", SourceId = "10" },
                        new ExternalTodoItem { Id = "ext-item-11b", SourceId = "11" },
                    },
                }
            );

        await CreateHandler(context, client.Object)
            .Handle(new RunSyncCommand(), CancellationToken.None);

        client.Verify(
            c => c.DeleteTodoListAsync("ext-1", It.IsAny<CancellationToken>()),
            Times.Once
        );

        var list = await context.TodoLists.Include(l => l.Items).SingleAsync();
        Assert.Equal("ext-1b", list.ExternalId);
        Assert.Equal("ext-item-11b", list.Items.Single(i => i.Id == 11).ExternalId);
    }
}
