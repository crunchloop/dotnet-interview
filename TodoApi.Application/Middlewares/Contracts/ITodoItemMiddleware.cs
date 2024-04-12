using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.Application.Middlewares.Contracts
{
    public interface ITodoItemMiddleware
    {
        Task<List<TodoItem>> GetAllItems(int listId);

        Task CreateItem(TodoItem item);
    }
}
