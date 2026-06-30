using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Todo.Application.Common.Interfaces;
using Todo.Application.Integration.ExternalApi.Mapping;
using Todo.Application.Sync.Notifications;

namespace Todo.Application.Sync.Outbound;

/// <summary>
/// Pushes a newly created local list (with its current items) to the external API and
/// persists the returned external ids so future syncs correlate by id.
/// </summary>
public class TodoListCreatedNotificationHandler : INotificationHandler<TodoListCreatedNotification>
{
    private readonly IApplicationDbContext _context;
    private readonly IExternalTodoApiClient _client;
    private readonly ILogger<TodoListCreatedNotificationHandler> _logger;

    public TodoListCreatedNotificationHandler(
        IApplicationDbContext context,
        IExternalTodoApiClient client,
        ILogger<TodoListCreatedNotificationHandler> logger
    )
    {
        _context = context;
        _client = client;
        _logger = logger;
    }

    public async Task Handle(
        TodoListCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var list = await _context
                .TodoLists.Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.Id == notification.TodoListId, cancellationToken);

            if (list is null || list.ExternalId is not null)
            {
                return;
            }

            var created = await _client.CreateTodoListAsync(list.ToCreateBody(), cancellationToken);

            list.ExternalId = created.Id;

            // Correlate returned item ids back to our local items via source_id.
            foreach (var externalItem in created.Items)
            {
                if (!long.TryParse(externalItem.SourceId, out var localItemId))
                {
                    continue;
                }

                var localItem = list.Items.FirstOrDefault(i => i.Id == localItemId);
                if (localItem is not null)
                {
                    localItem.ExternalId = externalItem.Id;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug(
                "Synced created todo list {TodoListId} to external {ExternalId}.",
                list.Id,
                created.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to push created todo list {TodoListId} to the external API.",
                notification.TodoListId
            );
        }
    }
}
