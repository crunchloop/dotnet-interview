using Microsoft.AspNetCore.SignalR;
using Todo.Application.BulkCompletion.Dtos;
using Todo.Application.Common.Interfaces;

namespace Todo.Web.Hubs;

/// <summary>
/// SignalR-backed implementation of <see cref="ITodoCompletionNotifier"/>. Each
/// message is broadcast to the <c>todo-{todoListId}</c> group.
/// </summary>
public class TodoCompletionNotifier : ITodoCompletionNotifier
{
    private readonly IHubContext<TodoCompletionHub> _hubContext;

    public TodoCompletionNotifier(IHubContext<TodoCompletionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task JobStartedAsync(
        long todoListId,
        JobStartedDto message,
        CancellationToken cancellationToken = default
    ) => Group(todoListId).SendAsync("JobStarted", message, cancellationToken);

    public Task ProgressUpdatedAsync(
        long todoListId,
        ProgressUpdateDto message,
        CancellationToken cancellationToken = default
    ) => Group(todoListId).SendAsync("ProgressUpdate", message, cancellationToken);

    public Task JobCompletedAsync(
        long todoListId,
        JobCompletedDto message,
        CancellationToken cancellationToken = default
    ) => Group(todoListId).SendAsync("JobCompleted", message, cancellationToken);

    public Task JobFailedAsync(
        long todoListId,
        JobFailedDto message,
        CancellationToken cancellationToken = default
    ) => Group(todoListId).SendAsync("JobFailed", message, cancellationToken);

    public Task JobCancelledAsync(
        long todoListId,
        JobCancelledDto message,
        CancellationToken cancellationToken = default
    ) => Group(todoListId).SendAsync("JobCancelled", message, cancellationToken);

    private IClientProxy Group(long todoListId) =>
        _hubContext.Clients.Group(TodoCompletionHub.GroupName(todoListId));
}
