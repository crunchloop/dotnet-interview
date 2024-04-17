using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApi.Application.ViewModels
{
    public class CreateTodoItemViewModel
    {
        public string ItemName { get; set; }

        public int TodoListId { get; set; }
    }
}
