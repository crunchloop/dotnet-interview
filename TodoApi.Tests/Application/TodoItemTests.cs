using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Application.Middlewares;
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
    }
}
