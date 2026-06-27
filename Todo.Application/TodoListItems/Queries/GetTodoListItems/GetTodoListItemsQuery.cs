using MediatR;
using Todo.Application.Common.Dtos;

namespace Todo.Application.TodoListItems.Queries.GetTodoListItems;

public class GetTodoListItemsQuery : IRequest<List<TodoListItemDto>?>
{
    public long TodoListId { get; set; }
}
