using Microsoft.EntityFrameworkCore;
using Todo.Application.TodoListItems.Queries.GetTodoListItemById;
using Todo.Application.TodoListItems.Queries.GetTodoListItems;
using Todo.Domain.Entities;
using Todo.Persistence;

namespace Todo.Application.Tests.TodoListItems;

public class TodoListItemQueryHandlerTests
{
    private static DbContextOptions<TodoContext> DatabaseContextOptions() =>
        new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoLists.Add(new TodoList { Id = 1, Name = "Task 1" });
        // An empty list lets us distinguish "no items" (empty) from "no list" (null).
        context.TodoLists.Add(new TodoList { Id = 2, Name = "Empty List" });
        context.TodoListItems.Add(
            new TodoListItem
            {
                Id = 1,
                Name = "Item 1",
                TodoListId = 1,
                IsComplete = true,
            }
        );
        context.TodoListItems.Add(
            new TodoListItem
            {
                Id = 2,
                Name = "Item 2",
                TodoListId = 1,
            }
        );
        context.SaveChanges();
    }

    [Fact]
    public async Task GetTodoListItems_WhenTodoListDoesntExist_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListItemsQueryHandler(context);

        var result = await handler.Handle(
            new GetTodoListItemsQuery { TodoListId = 99 },
            CancellationToken.None
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTodoListItems_WhenListExistsButHasNoItems_ReturnsEmptyList()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListItemsQueryHandler(context);

        var result = await handler.Handle(
            new GetTodoListItemsQuery { TodoListId = 2 },
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.Empty(result!);
    }

    [Fact]
    public async Task GetTodoListItems_WhenCalled_ReturnsOnlyThatListsItemsWithMappedFields()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListItemsQueryHandler(context);

        var result = await handler.Handle(
            new GetTodoListItemsQuery { TodoListId = 1 },
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.All(result, dto => Assert.Equal(1, dto.TodoListId));

        var completed = result.Single(dto => dto.Id == 1);
        Assert.Equal("Item 1", completed.Name);
        Assert.True(completed.IsComplete);
        Assert.False(result.Single(dto => dto.Id == 2).IsComplete);
    }

    [Fact]
    public async Task GetTodoListItemById_WhenCalled_ReturnsTodoListItemById()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListItemByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetTodoListItemByIdQuery { Id = 1, TodoListId = 1 },
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Item 1", result.Name);
        Assert.True(result.IsComplete);
    }

    [Fact]
    public async Task GetTodoListItemById_WhenTodoListItemDoesntExist_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListItemByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetTodoListItemByIdQuery { Id = 99, TodoListId = 1 },
            CancellationToken.None
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTodoListItemById_WhenItemBelongsToAnotherList_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListItemByIdQueryHandler(context);

        // Item 1 exists under list 1; querying it under list 2 must not match.
        var result = await handler.Handle(
            new GetTodoListItemByIdQuery { Id = 1, TodoListId = 2 },
            CancellationToken.None
        );

        Assert.Null(result);
    }
}
