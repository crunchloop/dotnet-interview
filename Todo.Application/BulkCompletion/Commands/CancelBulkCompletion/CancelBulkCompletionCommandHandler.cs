using MediatR;
using Todo.Application.Common.Interfaces;

namespace Todo.Application.BulkCompletion.Commands.CancelBulkCompletion;

public class CancelBulkCompletionCommandHandler : IRequestHandler<CancelBulkCompletionCommand, bool>
{
    private readonly IBulkCompletionService _bulkCompletionService;

    public CancelBulkCompletionCommandHandler(IBulkCompletionService bulkCompletionService)
    {
        _bulkCompletionService = bulkCompletionService;
    }

    public Task<bool> Handle(
        CancelBulkCompletionCommand request,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(_bulkCompletionService.CancelJob(request.JobId));
    }
}
