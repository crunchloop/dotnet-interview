using MediatR;
using Microsoft.Extensions.Logging;
using Todo.Application.Common.Interfaces;
using Todo.Application.Sync.Notifications;

namespace Todo.Application.Sync.Outbound;

/// <summary>Propagates a local item deletion to the external API, if it was synced.</summary>
public class TodoListItemDeletedNotificationHandler
    : INotificationHandler<TodoListItemDeletedNotification>
{
    private readonly IExternalTodoApiClient _client;
    private readonly ILogger<TodoListItemDeletedNotificationHandler> _logger;

    public TodoListItemDeletedNotificationHandler(
        IExternalTodoApiClient client,
        ILogger<TodoListItemDeletedNotificationHandler> logger
    )
    {
        _client = client;
        _logger = logger;
    }

    public async Task Handle(
        TodoListItemDeletedNotification notification,
        CancellationToken cancellationToken
    )
    {
        if (notification.ListExternalId is null || notification.ItemExternalId is null)
        {
            return;
        }

        try
        {
            await _client.DeleteTodoItemAsync(
                notification.ListExternalId,
                notification.ItemExternalId,
                cancellationToken
            );

            _logger.LogDebug(
                "Synced deletion of external todo item {ItemExternalId} on list {ListExternalId}.",
                notification.ItemExternalId,
                notification.ListExternalId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to push deletion of external todo item {ItemExternalId}.",
                notification.ItemExternalId
            );
        }
    }
}
