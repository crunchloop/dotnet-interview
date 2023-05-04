using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Models;

namespace TodoApi.Tests;

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
        context.TodoList.Add(new Models.TodoList
        {
            Id = 1,
            Name = "Task 1",
            Tasks = new List<TodoItem>() { new TodoItem { Id = 1, Name = "Task 1", Description = "Description 1" } },
        });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetTodoItems_WhenCalled_ReturnsTodoItemsList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoTaskController(context);

            var result = await controller.GetTodoTasks(1);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(
                1,
                ((result.Result as OkObjectResult).Value as IList<TodoItem>).Count
            );
        }
    }
}