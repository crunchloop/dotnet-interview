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
        context.TodoListItems.Add(
            new TodoListItem
            {
                Id = 1,
                Name = "Item 1",
                TodoListId = 1,
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
            new GetTodoListItemsQuery { TodoListId = 2 },
            CancellationToken.None
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTodoListItems_WhenCalled_ReturnsTodoListItemList()
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
    }

    [Fact]
    public async Task GetTodoListItemById_WhenTodoListItemDoesntExist_ReturnsNull()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);

        var handler = new GetTodoListItemByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetTodoListItemByIdQuery { Id = 3, TodoListId = 1 },
            CancellationToken.None
        );

        Assert.Null(result);
    }
}
