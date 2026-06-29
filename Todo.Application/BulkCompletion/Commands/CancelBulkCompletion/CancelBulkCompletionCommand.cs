using MediatR;

namespace Todo.Application.BulkCompletion.Commands.CancelBulkCompletion;

public class CancelBulkCompletionCommand : IRequest<bool>
{
    public Guid JobId { get; set; }
}
