using MediatR;

namespace Todo.Application.TodoListItems.Commands.DeleteTodoListItem;

public class DeleteTodoListItemCommand : IRequest
{
    public long Id { get; set; }
    public long TodoListId { get; set; }
}
