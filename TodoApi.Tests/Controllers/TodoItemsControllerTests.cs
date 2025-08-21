using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using TodoApi.Controllers;
using TodoApi.Dtos.Request;
using TodoApi.Dtos.Response;
using TodoApi.Models;

namespace TodoApi.Tests.Controllers;

public class TodoItemsControllerTests
{
    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoList.Add(new TodoList { Id = 1, Name = "Task 1" });
        context.TodoList.Add(new TodoList { Id = 2, Name = "Task 2" });
        context.TodoList.Add(new TodoList { Id = 3, Name = "Task 3" });
        context.TodoItem.AddRange([
            new TodoItem { Id = 1, Name = "Item 1", IsComplete = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, TodoListId = 1 },
            new TodoItem { Id = 2, Name = "Item 2", IsComplete = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, TodoListId = 1 },
            new TodoItem { Id = 3, Name = "Item 3", IsComplete = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, TodoListId = 1 },
            new TodoItem { Id = 4, Name = "Item 4", IsComplete = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, TodoListId = 2 },
            new TodoItem { Id = 5, Name = "Item 5", IsComplete = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, TodoListId = 2 }
        ]);

        context.SaveChanges();
    }

    #region GetTodoItems Scenarios

    [Fact]
    public async Task GetTodoItems_WhenCalled_ReturnsTodoItemsFromList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            int todoListId = 1;
            var controller = new TodoItemsController(context);

            // Act
            var result = await controller.GetTodoItems(todoListId);
            var okResult = result?.Result as OkObjectResult;
            var items = okResult?.Value as IEnumerable<TodoItemResponseDto>;

            // Assert
            Assert.IsType<OkObjectResult>(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult?.StatusCode);
            Assert.Equal(3, items?.Count());
        }
    }


    [Fact]
    public async Task GetTodoItems_WhenCalled_Returns404()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            int todoListId = 1000;
            var controller = new TodoItemsController(context);

            // Act
            var result = await controller.GetTodoItems(todoListId);
            var failedResult = result?.Result as NotFoundObjectResult;

            // Assert
            Assert.IsType<NotFoundObjectResult>(failedResult);
            Assert.Equal(StatusCodes.Status404NotFound, failedResult?.StatusCode);
        }
    }

    #endregion

    #region GetTodoItem Scenarios

    [Fact]
    public async Task GetTodoItem_WhenCalled_ReturnsTodoItemsFromList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            int todoListId = 1;
            int todoItemId = 3;
            var controller = new TodoItemsController(context);

            // Act
            var result = await controller.GetTodoItem(todoListId, todoItemId);
            var okResult = result?.Result as OkObjectResult;
            var item = okResult?.Value as TodoItemResponseDto;

            // Assert
            string parsedResponse = JsonSerializer.Serialize(item);
            string expectedResponse = JsonSerializer.Serialize(new TodoItemResponseDto(todoItemId, "Item 3", false, todoListId));

            Assert.IsType<OkObjectResult>(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult?.StatusCode);
            Assert.Equal(expectedResponse, parsedResponse);
        }
    }


    [Fact]
    public async Task GetTodoItem_WhenCalled_Returns404()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            int todoListId = 2;
            int todoItemId = 2000;
            var controller = new TodoItemsController(context);

            // Act
            var result = await controller.GetTodoItem(todoListId, todoItemId);
            var failedResult = result?.Result as NotFoundObjectResult;

            // Assert
            Assert.IsType<NotFoundObjectResult>(failedResult);
            Assert.Equal(StatusCodes.Status404NotFound, failedResult?.StatusCode);
        }
    }

    #endregion

    #region CreateTodoItem Scenarios

    [Fact]
    public async Task CreateTodoItem_WhenCalled_ReturnsAddedItem()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            CreateTodoItem todoItemNew = new() { Name = "Item from Unit Test", IsComplete = false, TodoListId = 2 };

            // Act
            var result = await controller.CreateTodoItem(todoItemNew);
            var okResult = result?.Result as CreatedAtActionResult;
            var item = okResult?.Value as TodoItemResponseDto;

            // Assert
            string parsedResponse = JsonSerializer.Serialize(item);
            string expectedResponse = JsonSerializer.Serialize(new TodoItemResponseDto(6, "Item from Unit Test", false, 2));

            Assert.IsType<CreatedAtActionResult>(okResult);
            Assert.Equal(StatusCodes.Status201Created, okResult?.StatusCode);
            Assert.Equal(expectedResponse, parsedResponse);
        }
    }


    [Fact]
    public async Task CreateTodoItem_WhenCalled_Returns400()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            CreateTodoItem todoItemNewFailed = new() { Name = "Item from Unit Test with unexisting todo list", IsComplete = true, TodoListId = 1000 };

            // Act
            var result = await controller.CreateTodoItem(todoItemNewFailed);
            var failedResult = result?.Result as BadRequestObjectResult;

            // Assert
            Assert.IsType<BadRequestObjectResult>(failedResult);
            Assert.Equal(StatusCodes.Status400BadRequest, failedResult?.StatusCode);
        }
    }

    [Fact]
    public async Task CreateTodoItem_WhenCalled_Returns405_Conflict()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            CreateTodoItem todoItemDuplicatedName = new() { Name = "Item 5", IsComplete = true, TodoListId = 2 };

            // Act
            var result = await controller.CreateTodoItem(todoItemDuplicatedName);
            var failedResult = result?.Result as ConflictObjectResult;

            // Assert
            Assert.IsType<ConflictObjectResult>(failedResult);
            Assert.Equal(StatusCodes.Status409Conflict, failedResult?.StatusCode);
        }
    }

    #endregion

    #region UpdateTodoItem Scenarios

    [Fact]
    public async Task UpdateTodoItem_WhenCalled_ReturnsUpdatedItem()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            UpdateTodoItem todoItemUpdate = new() { Id = 2, Name = "Item 2 with name changed", TodoListId = 1 };

            // Act
            var result = await controller.UpdateTodoItem(todoItemUpdate);
            var okResult = result?.Result as OkObjectResult;

            // Assert
            string parsedResponse = JsonSerializer.Serialize(okResult?.Value);
            string expectedResponse = JsonSerializer.Serialize(new TodoItemResponseDto(2, "Item 2 with name changed", true, 1));

            Assert.IsType<OkObjectResult>(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult?.StatusCode);
            Assert.Equal(expectedResponse, parsedResponse);
        }
    }

    [Fact]
    public async Task UpdateTodoItem_WhenCalled_Returns404()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            UpdateTodoItem todoItemUpdateFailed = new() { Id = 1000, Name = "Item 2 with name changed", TodoListId = 2 };

            // Act
            var result = await controller.UpdateTodoItem(todoItemUpdateFailed);
            var failedResult = result?.Result as NotFoundObjectResult;

            // Assert
            Assert.IsType<NotFoundObjectResult>(failedResult);
            Assert.Equal(StatusCodes.Status404NotFound, failedResult?.StatusCode);
        }
    }

    #endregion

    #region DeleteTodoItem Scenarios

    [Fact]
    public async Task DeleteTodoItem_WhenCalled_RemovesTodoItemFromList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            int todoListId = 1;
            int todoItemId = 3;

            // Act
            var result = await controller.DeleteTodoItem(todoListId, todoItemId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2, context.TodoItem.Where(i => i.TodoListId == todoListId).Count());
            Assert.Equal(StatusCodes.Status200OK, (result as OkObjectResult)?.StatusCode);
        }
    }

    [Fact]
    public async Task DeleteTodoItem_WhenCalled_Returns404()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            int todoListId = 1;
            int todoItemId = 1500;

            // Act
            var result = await controller.DeleteTodoItem(todoListId, todoItemId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, (result as NotFoundObjectResult)?.StatusCode);
        }
    }

    #endregion

    #region DeleteTodoItems Scenarios

    [Fact]
    public async Task DeleteTodoItems_WhenCalled_RemovesTodoItemsFromList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            int todoListId = 1;

            // Act
            var result = await controller.DeleteTodoItems(todoListId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0, context.TodoItem.Where(ti => ti.TodoListId == todoListId).Count());
            Assert.Equal(StatusCodes.Status200OK, (result as OkObjectResult)?.StatusCode);
        }
    }

    [Fact]
    public async Task DeleteTodoItems_WhenCalled_ReturnsOkWithoutChanges()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            int todoListId = 3;

            // Act
            var result = await controller.DeleteTodoItems(todoListId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0, context.TodoItem.Where(ti => ti.TodoListId == todoListId).Count());
            Assert.Equal(StatusCodes.Status200OK, (result as OkObjectResult)?.StatusCode);
            Assert.Equal("Todo list is already empty!", (result as OkObjectResult)?.Value);
        }
    }

    [Fact]
    public async Task DeleteTodoItems_WhenCalled_Returns404()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            // Arrange
            PopulateDatabaseContext(context);
            var controller = new TodoItemsController(context);
            int todoListId = 1000;

            // Act
            var result = await controller.DeleteTodoItems(todoListId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, (result as NotFoundObjectResult)?.StatusCode);
        }
    }

    #endregion
}
