using MediatR;

namespace Todo.Application.BulkCompletion.Commands.StartBulkCompletion;

public class StartBulkCompletionCommand : IRequest<Guid>
{
    public long TodoListId { get; set; }
}
