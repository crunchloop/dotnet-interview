using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SQLite;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi.Aplication.Services;
using TodoApi.Application.AutoMapper;
using TodoApi.Application.Hubs;
using TodoApi.Application.Interfaces;
using TodoApi.Application.Services;
using TodoApi.Data;
using TodoApi.Infrastructure.Interfaces;
using TodoApi.Infrastructure.Repositories;
using TodoApi.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddDbContext<TodoContext>(opt =>
        opt.UseSqlite(builder.Configuration.GetConnectionString("TodoContext"),
        b => b.MigrationsAssembly("TodoApi.Infrastructure"))
    )
    .AddEndpointsApiExplorer()
    .AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TodoApi",
        Version = "v1"
    });


});
builder.Services.AddSignalR();

builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
builder.Services.AddScoped<ITodoListRepository, TodoListRepository>();

builder.Services.AddScoped<TodoListExistsFilter>();

builder.Services.AddScoped<ITodoItemService, TodoItemService>();
builder.Services.AddScoped<ITodoListService, TodoListService>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage()
    .UseFilter(new AutomaticRetryAttribute
    {
        Attempts = 1, 
        DelaysInSeconds = new int[] { 300 }  
    });
});

builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowCredentials();
    });
});

var app = builder.Build();
app.UseHangfireDashboard();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApi");
        c.RoutePrefix = string.Empty;
    });
}
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error != null)
        {
            await context.Response.WriteAsync(new
            {
                Error = "Ocurrió un error inesperado",
            }.ToString());
        }
    });
});

app.UseHangfireDashboard();

app.MapHub<NotificationHub>("/notificationHub");
app.UseCors("AllowAll");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
