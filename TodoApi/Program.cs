using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddDbContext<TodoContext>(
        // Use SQL Server
        // opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext"));
        opt => opt.UseInMemoryDatabase("TodoList")
    )
    .AddEndpointsApiExplorer()
    .AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddTransient<ITodoItemRepository, TodoItemRepository>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

var scope = app.Services.CreateScope();

var context = scope.ServiceProvider.GetRequiredService<TodoContext>();

context.TodoList.Add(new TodoList { Id = 1, Name = "List 1", Items = new List<TodoItem>() { new TodoItem { Id = 1, ItemName = "Task 1", TodoListId = 1} } });
context.TodoList.Add(new TodoList { Id = 2, Name = "List 2" });
context.SaveChanges();

app.Run();