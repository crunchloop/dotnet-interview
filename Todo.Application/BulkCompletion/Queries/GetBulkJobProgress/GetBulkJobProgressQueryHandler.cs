using MediatR;
using Todo.Application.BulkCompletion.Dtos;
using Todo.Application.Common.Interfaces;

namespace Todo.Application.BulkCompletion.Queries.GetBulkJobProgress;

public class GetBulkJobProgressQueryHandler
    : IRequestHandler<GetBulkJobProgressQuery, JobProgressDto?>
{
    private readonly IBulkCompletionService _bulkCompletionService;

    public GetBulkJobProgressQueryHandler(IBulkCompletionService bulkCompletionService)
    {
        _bulkCompletionService = bulkCompletionService;
    }

    public Task<JobProgressDto?> Handle(
        GetBulkJobProgressQuery request,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(_bulkCompletionService.GetProgress(request.JobId));
    }
}
