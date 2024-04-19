using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

//Agrega los controladores de la API
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

            

//Agrega enpoints
builder.Services.AddEndpointsApiExplorer();

//Agrega el Swagger
builder.Services.AddSwaggerGen();

builder.Services
    .AddDbContext<TodoContext>(
        // Use SQL Server
        opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext"))//.UseLazyLoadingProxies()
        // Carga en memoria la base
        //opt => opt.UseInMemoryDatabase("TodoList")
    )
    .AddEndpointsApiExplorer()
    .AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

#region Swagger
app.UseSwagger();
app.UseSwaggerUI();
#endregion

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<TodoContext>();
//
//    context.TodoList.Add(
//        new TodoApi.Models.TodoList { Id = 1, Name = "List1" }
//    );
//
//    context.TodoList.Add(
//        new TodoApi.Models.TodoList { Id = 2, Name = "List2", Items = new List<TodoItem>() { new TodoItem() { Id = 1, Title = "Item1", Description = "Desc", Completed = false } } }
//    );
//
//    context.SaveChanges();
//}

app.UseAuthorization();
app.MapControllers();
app.Run();