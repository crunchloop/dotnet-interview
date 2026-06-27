using MediatR;
using Todo.Application.Common.Dtos;

namespace Todo.Application.TodoLists.Queries.GetTodoListById;

public class GetTodoListByIdQuery : IRequest<TodoListDto?>
{
    public long Id { get; set; }
}
