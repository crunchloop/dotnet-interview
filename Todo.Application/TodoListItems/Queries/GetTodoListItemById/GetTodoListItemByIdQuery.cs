using MediatR;
using Todo.Application.Common.Dtos;

namespace Todo.Application.TodoListItems.Queries.GetTodoListItemById;

public class GetTodoListItemByIdQuery : IRequest<TodoListItemDto?>
{
    public long Id { get; set; }
    public long TodoListId { get; set; }
}
