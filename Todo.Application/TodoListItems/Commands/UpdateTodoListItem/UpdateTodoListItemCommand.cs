using MediatR;

namespace Todo.Application.TodoListItems.Commands.UpdateTodoListItem;

public class UpdateTodoListItemCommand : IRequest
{
    public long Id { get; set; }
    public long TodoListId { get; set; }
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
}
