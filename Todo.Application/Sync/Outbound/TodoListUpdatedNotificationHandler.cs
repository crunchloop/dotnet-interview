using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Todo.Application.Common.Interfaces;
using Todo.Application.Integration.ExternalApi.Mapping;
using Todo.Application.Sync.Notifications;

namespace Todo.Application.Sync.Outbound;

/// <summary>Propagates a local list update to the external API, if already synced.</summary>
public class TodoListUpdatedNotificationHandler : INotificationHandler<TodoListUpdatedNotification>
{
    private readonly IApplicationDbContext _context;
    private readonly IExternalTodoApiClient _client;
    private readonly ILogger<TodoListUpdatedNotificationHandler> _logger;

    public TodoListUpdatedNotificationHandler(
        IApplicationDbContext context,
        IExternalTodoApiClient client,
        ILogger<TodoListUpdatedNotificationHandler> logger
    )
    {
        _context = context;
        _client = client;
        _logger = logger;
    }

    public async Task Handle(
        TodoListUpdatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var list = await _context.TodoLists.FirstOrDefaultAsync(
                l => l.Id == notification.TodoListId,
                cancellationToken
            );

            if (list?.ExternalId is null)
            {
                return;
            }

            await _client.UpdateTodoListAsync(
                list.ExternalId,
                list.ToUpdateBody(),
                cancellationToken
            );

            _logger.LogDebug(
                "Synced updated todo list {TodoListId} to external {ExternalId}.",
                list.Id,
                list.ExternalId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to push updated todo list {TodoListId} to the external API.",
                notification.TodoListId
            );
        }
    }
}
