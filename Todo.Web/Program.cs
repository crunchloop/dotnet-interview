using Todo.Application;
using Todo.Infrastructure;
using Todo.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
