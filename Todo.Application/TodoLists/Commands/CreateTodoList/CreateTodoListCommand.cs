using MediatR;

namespace Todo.Application.TodoLists.Commands.CreateTodoList;

public class CreateTodoListCommand : IRequest<long>
{
    public required string Name { get; set; }
}
