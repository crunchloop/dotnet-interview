using Todo.Application;
using Todo.Application.Common.Interfaces;
using Todo.Infrastructure;
using Todo.Web.Hubs;
using Todo.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddControllers();

builder.Services.AddSignalR();
builder.Services.AddSingleton<ITodoCompletionNotifier, TodoCompletionNotifier>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHub<TodoCompletionHub>("/hubs/todo-completion");
app.Run();
