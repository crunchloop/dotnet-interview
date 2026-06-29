using Todo.Application.BulkCompletion.Dtos;

namespace Todo.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the real-time transport (SignalR) so the background service
/// stays free of any web/transport dependency. The Web layer implements this on
/// top of the SignalR hub context.
/// </summary>
public interface ITodoCompletionNotifier
{
    Task JobStartedAsync(
        long todoListId,
        JobStartedDto message,
        CancellationToken cancellationToken = default
    );
    Task ProgressUpdatedAsync(
        long todoListId,
        ProgressUpdateDto message,
        CancellationToken cancellationToken = default
    );
    Task JobCompletedAsync(
        long todoListId,
        JobCompletedDto message,
        CancellationToken cancellationToken = default
    );
    Task JobFailedAsync(
        long todoListId,
        JobFailedDto message,
        CancellationToken cancellationToken = default
    );
    Task JobCancelledAsync(
        long todoListId,
        JobCancelledDto message,
        CancellationToken cancellationToken = default
    );
}
