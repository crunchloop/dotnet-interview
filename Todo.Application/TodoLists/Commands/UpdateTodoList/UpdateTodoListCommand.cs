using MediatR;

namespace Todo.Application.TodoLists.Commands.UpdateTodoList;

public class UpdateTodoListCommand : IRequest
{
    public long Id { get; set; }
    public required string Name { get; set; }
}
