using MediatR;

namespace Todo.Application.TodoLists.Commands.DeleteTodoList;

public class DeleteTodoListCommand : IRequest
{
    public long Id { get; set; }
}
