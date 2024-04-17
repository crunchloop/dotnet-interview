using FluentAssertions;
using NSubstitute;
using System;
using TodoApi.Application.Middlewares;
using TodoApi.Application.ViewModels;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Tests.Application
{
    public class TodoItemTests
    {
        private readonly TodoItemMiddleware _sut;
        private readonly ITodoItemRepository _todoItemRepository = Substitute.For<ITodoItemRepository>();

        public TodoItemTests()
        {
            this._sut = new TodoItemMiddleware(this._todoItemRepository);
        }

        [Fact]
        public async Task GetAllItemsByID_ShouldReturn_WhenListContainsItems() 
        {
            var items = new List<TodoItem>();
            var todoItem = new TodoItem
            {
                Id = 1,
                ItemName = "Task 2",
                TodoListId = 1,
            };

            items.Add(todoItem);

            this._todoItemRepository.GetAllItems(1).Returns(items);

            var result = await this._sut.GetAllItems(1);
            result.Should().NotBeEmpty();
        }


        [Fact]
        public async Task GetAllItemsByID_ShouldReturn_EmptyListOnce()
        {
            var items = new List<TodoItem>();
            var todoItem = new TodoItem
            {
                Id = 1,
                ItemName = "Task 2",
                TodoListId = 1,
            };

            items.Add(todoItem);

            this._todoItemRepository.GetAllItems(1).Returns(new List<TodoItem>());

            var result = await this._sut.GetAllItems(1);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateItem_ShouldReturnId_OnceItemIsCreated()
        {

            var todoItemViewModel = new CreateTodoItemViewModel
            {
                TodoListId = 1,
                ItemName = "Test"
            };

            var todoItem = new TodoItem
            {
                ItemName = "Task 2",
                TodoListId = 1,
            };

            this._todoItemRepository.CreateItem(todoItem).Returns(1);
            var result = await this._sut.CreateItem(todoItemViewModel);
            result.Should().BeGreaterThan(0);
        }


        [Fact]
        public async Task UpdateItem_ShouldThrowException_Once_CannotFoundItem_InDatabase()
        {
            this._todoItemRepository.UpdateItem(2, "Test").Returns(x => { throw new Exception("Item not found"); });
            var action = async () => await this._sut.UpdateItem(2, new UpdateTodoItemViewModel { ItemName = "Test"});
            var exception = await Assert.ThrowsAnyAsync<Exception>(action);
            Assert.Equal("Item not found", exception.Message);
        }


        [Fact]
        public async Task DeleteItems_ShouldThrowException_Once_IdISEqualZero()
        {
            this._todoItemRepository.DeleteItem(0).Returns(x => { throw new Exception("Item id is null or equal zero"); });
            var action = async () => await this._sut.DeleteItem(0);
            var exception = await Assert.ThrowsAnyAsync<Exception>(action);
            Assert.Equal("Item id is null or equal zero", exception.Message);
        }
    }
}
