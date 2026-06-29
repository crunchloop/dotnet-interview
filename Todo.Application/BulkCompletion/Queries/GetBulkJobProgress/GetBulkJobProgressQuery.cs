using MediatR;
using Todo.Application.BulkCompletion.Dtos;

namespace Todo.Application.BulkCompletion.Queries.GetBulkJobProgress;

public class GetBulkJobProgressQuery : IRequest<JobProgressDto?>
{
    public Guid JobId { get; set; }
}
