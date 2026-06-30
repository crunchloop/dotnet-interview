using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Todo.Application.Common.Interfaces;
using Todo.Application.Integration.ExternalApi.Mapping;
using Todo.Application.Sync.Notifications;

namespace Todo.Application.Sync.Outbound;

/// <summary>
/// Propagates a local item update to the external API, but only when both the parent
/// list and the item have already been synced (the external API addresses items by
/// their external ids, nested under the list).
/// </summary>
public class TodoListItemUpdatedNotificationHandler
    : INotificationHandler<TodoListItemUpdatedNotification>
{
    private readonly IApplicationDbContext _context;
    private readonly IExternalTodoApiClient _client;
    private readonly ILogger<TodoListItemUpdatedNotificationHandler> _logger;

    public TodoListItemUpdatedNotificationHandler(
        IApplicationDbContext context,
        IExternalTodoApiClient client,
        ILogger<TodoListItemUpdatedNotificationHandler> logger
    )
    {
        _context = context;
        _client = client;
        _logger = logger;
    }

    public async Task Handle(
        TodoListItemUpdatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var item = await _context
                .TodoListItems.Include(i => i.TodoList)
                .FirstOrDefaultAsync(i => i.Id == notification.TodoListItemId, cancellationToken);

            if (item?.ExternalId is null || item.TodoList?.ExternalId is null)
            {
                return;
            }

            await _client.UpdateTodoItemAsync(
                item.TodoList.ExternalId,
                item.ExternalId,
                item.ToUpdateBody(),
                cancellationToken
            );

            _logger.LogDebug(
                "Synced updated todo item {TodoListItemId} to external {ExternalId}.",
                item.Id,
                item.ExternalId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to push updated todo item {TodoListItemId} to the external API.",
                notification.TodoListItemId
            );
        }
    }
}
