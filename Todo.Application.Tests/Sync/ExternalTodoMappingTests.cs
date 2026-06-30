using Todo.Application.Integration.ExternalApi.Dtos;
using Todo.Application.Integration.ExternalApi.Mapping;
using Todo.Domain.Entities;

namespace Todo.Application.Tests.Sync;

public class ExternalTodoMappingTests
{
    [Fact]
    public void ToCreateBody_MapsListAndItems_UsingLocalIdsAsSourceIds()
    {
        var list = new TodoList
        {
            Id = 7,
            Name = "Groceries",
            Items =
            {
                new TodoListItem
                {
                    Id = 11,
                    Name = "Milk",
                    IsComplete = true,
                },
                new TodoListItem
                {
                    Id = 12,
                    Name = "Eggs",
                    IsComplete = false,
                },
            },
        };

        var body = list.ToCreateBody();

        Assert.Equal("Groceries", body.Name);
        Assert.Equal("7", body.SourceId);
        Assert.Equal(2, body.Items.Count);

        Assert.Equal("Milk", body.Items[0].Description);
        Assert.True(body.Items[0].Completed);
        Assert.Equal("11", body.Items[0].SourceId);

        Assert.Equal("Eggs", body.Items[1].Description);
        Assert.False(body.Items[1].Completed);
        Assert.Equal("12", body.Items[1].SourceId);
    }

    [Fact]
    public void ToUpdateBody_MapsNameAndItemFields()
    {
        var list = new TodoList { Id = 1, Name = "Renamed" };
        var item = new TodoListItem
        {
            Id = 2,
            Name = "Task",
            IsComplete = true,
        };

        Assert.Equal("Renamed", list.ToUpdateBody().Name);

        var itemBody = item.ToUpdateBody();
        Assert.Equal("Task", itemBody.Description);
        Assert.True(itemBody.Completed);
    }

    [Fact]
    public void ToNewTodoList_MapsFromExternal_PreservingExternalIdsAndTimestamps()
    {
        var created = DateTime.UtcNow.AddDays(-2);
        var updated = DateTime.UtcNow.AddDays(-1);

        var external = new ExternalTodoList
        {
            Id = "ext-list-1",
            SourceId = null,
            Name = "From Remote",
            CreatedAt = created,
            UpdatedAt = updated,
            Items =
            {
                new ExternalTodoItem
                {
                    Id = "ext-item-1",
                    Description = "Remote item",
                    Completed = true,
                    CreatedAt = created,
                    UpdatedAt = updated,
                },
            },
        };

        var list = external.ToNewTodoList();

        Assert.Equal("From Remote", list.Name);
        Assert.Equal("ext-list-1", list.ExternalId);
        Assert.Equal(created, list.CreatedAt);
        Assert.Equal(updated, list.UpdatedAt);

        var item = Assert.Single(list.Items);
        Assert.Equal("Remote item", item.Name);
        Assert.True(item.IsComplete);
        Assert.Equal("ext-item-1", item.ExternalId);
    }
}
