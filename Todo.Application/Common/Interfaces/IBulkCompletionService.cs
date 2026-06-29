using Todo.Application.BulkCompletion.Dtos;

namespace Todo.Application.Common.Interfaces;

/// <summary>
/// Orchestrates background bulk-completion jobs and exposes their progress.
/// Implemented as a singleton: it owns the in-memory job registry and spawns the
/// background processing tasks.
/// </summary>
public interface IBulkCompletionService
{
    /// <summary>Queues a job that completes every incomplete item of the list and returns its id.</summary>
    Guid StartJob(long todoListId);

    /// <summary>Returns the current snapshot of a job, or <c>null</c> if it is unknown.</summary>
    JobProgressDto? GetProgress(Guid jobId);

    /// <summary>Requests cancellation of a running job. Returns <c>false</c> if it is not cancellable.</summary>
    bool CancelJob(Guid jobId);
}
