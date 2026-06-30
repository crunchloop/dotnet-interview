using MediatR;
using Microsoft.Extensions.Logging;
using Todo.Application.Common.Interfaces;
using Todo.Application.Sync.Notifications;

namespace Todo.Application.Sync.Outbound;

/// <summary>Propagates a local list deletion to the external API, if it was synced.</summary>
public class TodoListDeletedNotificationHandler : INotificationHandler<TodoListDeletedNotification>
{
    private readonly IExternalTodoApiClient _client;
    private readonly ILogger<TodoListDeletedNotificationHandler> _logger;

    public TodoListDeletedNotificationHandler(
        IExternalTodoApiClient client,
        ILogger<TodoListDeletedNotificationHandler> logger
    )
    {
        _client = client;
        _logger = logger;
    }

    public async Task Handle(
        TodoListDeletedNotification notification,
        CancellationToken cancellationToken
    )
    {
        if (notification.ExternalId is null)
        {
            return;
        }

        try
        {
            await _client.DeleteTodoListAsync(notification.ExternalId, cancellationToken);

            _logger.LogDebug(
                "Synced deletion of external todo list {ExternalId}.",
                notification.ExternalId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to push deletion of external todo list {ExternalId}.",
                notification.ExternalId
            );
        }
    }
}
