using Microsoft.AspNetCore.SignalR;

namespace Todo.Web.Hubs;

/// <summary>
/// Real-time hub for bulk todo-completion progress. Clients join the group for a
/// given todo list to receive its JobStarted / ProgressUpdate / JobCompleted /
/// JobFailed / JobCancelled messages.
/// </summary>
public class TodoCompletionHub : Hub
{
    private readonly ILogger<TodoCompletionHub> _logger;

    public TodoCompletionHub(ILogger<TodoCompletionHub> logger)
    {
        _logger = logger;
    }

    /// <summary>Shared group-name convention so the hub and notifier stay in sync.</summary>
    public static string GroupName(long todoListId) => $"todo-{todoListId}";

    public Task JoinTodoGroup(long todoListId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(todoListId));
    }

    public Task LeaveTodoGroup(long todoListId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(todoListId));
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // SignalR removes the connection from its groups automatically; we only log.
        if (exception is not null)
        {
            _logger.LogWarning(
                exception,
                "Connection {ConnectionId} disconnected unexpectedly.",
                Context.ConnectionId
            );
        }

        return base.OnDisconnectedAsync(exception);
    }
}
