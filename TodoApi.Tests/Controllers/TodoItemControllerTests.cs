using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApi.Aplication.Services;
using TodoApi.Application.AutoMapper;
using TodoApi.Application.Dtos;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Tests;

public class TodoItemControllerTests
{
    private readonly IMapper _mapper;

    public TodoItemControllerTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.CreateMap<CreateItemDto, TodoItem>()
                .ForMember(dest => dest.TodoListId, opt => opt.Ignore());
            cfg.CreateMap<UpdateItemDto, TodoItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TodoListId, opt => opt.Ignore());
        }, loggerFactory);
        _mapper = config.CreateMapper();
    }

    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoList.Add(new TodoList { Id = 1, Name = "List 1" });
        context.TodoItem.Add(new TodoItem { Id = 1, TodoListId = 1, Name = "Item 1", Complete = false });
        context.TodoItem.Add(new TodoItem { Id = 2, TodoListId = 1, Name = "Item 2", Complete = true });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetItems_WhenCalled_ReturnsTodoItemsForList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var result = await controller.GetItems(1);

            Assert.IsType<OkObjectResult>(result.Result);
            var items = ((result.Result as OkObjectResult).Value as IEnumerable<TodoItemDto>);
            Assert.Equal(2, items.Count());
        }
    }

    [Fact]
    public async Task GetItems_WhenNoItemsExist_ReturnsNoContent()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            context.TodoList.Add(new TodoList { Id = 1, Name = "List 1" });
            context.SaveChanges();
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var result = await controller.GetItems(1);

            Assert.IsType<NoContentResult>(result.Result);
        }
    }

    [Fact]
    public async Task GetItem_WhenItemExists_ReturnsTodoItem()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var result = await controller.GetItem(1, 1);

            Assert.IsType<OkObjectResult>(result.Result);
            var item = ((result.Result as OkObjectResult).Value as TodoItemDto);
            Assert.Equal(1, item.Id);
            Assert.Equal("Item 1", item.Name);
        }
    }

    [Fact]
    public async Task GetItem_WhenItemDoesNotExist_ReturnsNoContent()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var result = await controller.GetItem(1, 999);

            Assert.IsType<NoContentResult>(result.Result);
        }
    }

    [Fact]
    public async Task CreateItem_WhenValid_CreatesTodoItem()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var createRequest = new CreateItemDto { Name = "New Item" };
            var result = await controller.CreateItem(1, createRequest);

            Assert.IsType<OkObjectResult>(result.Result);
            var operationResult = ((result.Result as OkObjectResult).Value as OperationResultDto);
            Assert.True(operationResult.Success);
            Assert.Contains("Iteam created with the id", operationResult.Message);
            Assert.Equal(3, context.TodoItem.Count());
        }
    }

    [Fact]
    public async Task CreateItem_WhenItemExists_ReturnsError()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var createRequest = new CreateItemDto { Name = "Item 1" }; // Duplicate name
            var result = await controller.CreateItem(1, createRequest);

            Assert.IsType<OkObjectResult>(result.Result);
            var operationResult = ((result.Result as OkObjectResult).Value as OperationResultDto);
            Assert.False(operationResult.Success);
            Assert.Equal("Item already create", operationResult.Message);
        }
    }

    [Fact]
    public async Task UpdateItem_WhenItemExists_UpdatesTodoItem()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var updateRequest = new UpdateItemDto { Name = "Updated Item", Complete = true };
            var result = await controller.UpdateItem(1, 1, updateRequest);

            Assert.IsType<OkObjectResult>(result);
            var operationResult = (result as OkObjectResult).Value as OperationResultDto;
            Assert.True(operationResult.Success);
            var updatedItem = await context.TodoItem.FirstAsync(x => x.Id == 1);
            Assert.Equal("Updated Item", updatedItem.Name);
            Assert.True(updatedItem.Complete);
        }
    }

    [Fact]
    public async Task UpdateItem_WhenItemDoesNotExist_ReturnsError()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var updateRequest = new UpdateItemDto { Name = "Non-existent Item" };
            var result = await controller.UpdateItem(1, 999, updateRequest);

            Assert.IsType<OkObjectResult>(result);
            var operationResult = (result as OkObjectResult).Value as OperationResultDto;
            Assert.False(operationResult.Success);
            Assert.Equal("Non-existent Item", operationResult.Message);
        }
    }

    [Fact]
    public async Task DeleteItem_WhenItemExists_RemovesTodoItem()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var result = await controller.DeleteItem(1, 1);

            Assert.IsType<OkObjectResult>(result);
            var operationResult = (result as OkObjectResult).Value as OperationResultDto;
            Assert.True(operationResult.Success);
            Assert.Equal(1, context.TodoItem.Count());
        }
    }

    [Fact]
    public async Task DeleteItem_WhenItemDoesNotExist_ReturnsError()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);
            var repository = new TodoItemRepository(context);
            var service = new TodoItemService(repository, _mapper);
            var controller = new TodoItemController(service);

            var result = await controller.DeleteItem(1, 999);

            Assert.IsType<OkObjectResult>(result);
            var operationResult = (result as OkObjectResult).Value as OperationResultDto;
            Assert.False(operationResult.Success);
            Assert.Equal("Non-existent Item", operationResult.Message);
        }
    }
}