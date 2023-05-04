using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

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

var app = builder.Build();
var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
context.TodoList.Add(new TodoList
{
    Id = 1, Name = "List 1",
    Tasks = new List<TodoItem>() { new TodoItem { Id = 1, Name = "Task 1", Description = "Description 1" } }
});
context.TodoList.Add(new TodoList { Id = 2, Name = "List 2" });
context.SaveChanges();
app.UseAuthorization();
app.MapControllers();
app.Run();