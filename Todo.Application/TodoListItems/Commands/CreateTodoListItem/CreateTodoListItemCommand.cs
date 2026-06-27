using MediatR;

namespace Todo.Application.TodoListItems.Commands.CreateTodoListItem;

public class CreateTodoListItemCommand : IRequest<long>
{
    public long TodoListId { get; set; }
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
}
