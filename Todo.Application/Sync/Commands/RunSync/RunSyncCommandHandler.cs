using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Todo.Application.Common.Interfaces;
using Todo.Application.Integration.ExternalApi.Dtos;
using Todo.Application.Integration.ExternalApi.Mapping;
using Todo.Application.Sync.Models;
using Todo.Domain.Entities;

namespace Todo.Application.Sync.Commands.RunSync;

/// <summary>
/// Reconciles local state with the external API. Correlation is by <c>source_id</c>
/// (our local id) when present, falling back to the stored external id; never by name.
/// All remote data is fetched in a single call and compared field-by-field so the sync
/// only writes to the external API when something actually changed.
/// </summary>
public class RunSyncCommandHandler : IRequestHandler<RunSyncCommand, SyncResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IExternalTodoApiClient _client;
    private readonly ILogger<RunSyncCommandHandler> _logger;

    public RunSyncCommandHandler(
        IApplicationDbContext context,
        IExternalTodoApiClient client,
        ILogger<RunSyncCommandHandler> logger
    )
    {
        _context = context;
        _client = client;
        _logger = logger;
    }

    public async Task<SyncResult> Handle(
        RunSyncCommand request,
        CancellationToken cancellationToken
    )
    {
        var result = new SyncResult();

        IReadOnlyList<ExternalTodoList> remoteLists;
        try
        {
            remoteLists = await _client.GetTodoListsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync aborted: failed to fetch remote todo lists.");
            result.Errors.Add($"Failed to fetch remote lists: {ex.Message}");
            return result;
        }

        var localLists = await _context
            .TodoLists.Include(l => l.Items)
            .ToListAsync(cancellationToken);

        var localById = localLists.ToDictionary(l => l.Id);
        var localByExternalId = localLists
            .Where(l => l.ExternalId is not null)
            .ToDictionary(l => l.ExternalId!);

        var matchedLocalIds = new HashSet<long>();

        // --- Inbound: reconcile each remote list against local state ---
        foreach (var remote in remoteLists)
        {
            try
            {
                var local = ResolveLocalList(remote, localById, localByExternalId);

                if (local is null)
                {
                    // Remote-origin list we have never seen: create it locally.
                    var created = remote.ToNewTodoList();
                    _context.TodoLists.Add(created);
                    result.ListsCreated++;
                    result.ItemsCreated += created.Items.Count;
                    continue;
                }

                matchedLocalIds.Add(local.Id);
                local.ExternalId ??= remote.Id;

                ReconcileListFields(local, remote, result);
                ReconcileItems(local, remote, result);

                // New local items the external API does not know about yet can only be
                // created through the list-creation flow, so recreate the list remotely.
                await PushNewItemsViaRecreateAsync(local, result, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to reconcile remote list {ExternalId} (source {SourceId}).",
                    remote.Id,
                    remote.SourceId
                );
                result.Errors.Add($"List {remote.Id}: {ex.Message}");
            }
        }

        // --- Outbound / deletions: local lists not present in the remote response ---
        foreach (var local in localLists)
        {
            if (matchedLocalIds.Contains(local.Id))
            {
                continue;
            }

            try
            {
                if (local.ExternalId is null)
                {
                    // Locally created, never synced: mirror it to the external API.
                    await PushNewListAsync(local, cancellationToken);
                }
                else
                {
                    // Previously synced but gone remotely: treat as a remote deletion.
                    _context.TodoLists.Remove(local);
                    result.ListsDeleted++;
                    result.ItemsDeleted += local.Items.Count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to reconcile local list {TodoListId} (external {ExternalId}).",
                    local.Id,
                    local.ExternalId
                );
                result.Errors.Add($"Local list {local.Id}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Sync complete. Lists +{LC}/~{LU}/-{LD}, Items +{IC}/~{IU}/-{ID}, errors {Errors}.",
            result.ListsCreated,
            result.ListsUpdated,
            result.ListsDeleted,
            result.ItemsCreated,
            result.ItemsUpdated,
            result.ItemsDeleted,
            result.Errors.Count
        );

        return result;
    }

    private static TodoList? ResolveLocalList(
        ExternalTodoList remote,
        IReadOnlyDictionary<long, TodoList> localById,
        IReadOnlyDictionary<string, TodoList> localByExternalId
    )
    {
        if (
            TryParseId(remote.SourceId, out var sourceId)
            && localById.TryGetValue(sourceId, out var bySource)
        )
        {
            return bySource;
        }

        return localByExternalId.TryGetValue(remote.Id, out var byExternal) ? byExternal : null;
    }

    private void ReconcileListFields(TodoList local, ExternalTodoList remote, SyncResult result)
    {
        // Only adopt remote values when remote is newer AND actually differs.
        if (remote.UpdatedAt > local.UpdatedAt && local.Name != remote.Name)
        {
            local.Name = remote.Name;
            result.ListsUpdated++;
        }
    }

    private void ReconcileItems(TodoList local, ExternalTodoList remote, SyncResult result)
    {
        var localItems = local.Items.ToList();
        var localItemById = localItems.ToDictionary(i => i.Id);
        var localItemByExternalId = localItems
            .Where(i => i.ExternalId is not null)
            .ToDictionary(i => i.ExternalId!);

        var matchedItemIds = new HashSet<long>();

        foreach (var remoteItem in remote.Items)
        {
            var localItem = ResolveLocalItem(remoteItem, localItemById, localItemByExternalId);

            if (localItem is null)
            {
                var created = remoteItem.ToNewTodoListItem();
                created.TodoListId = local.Id;
                local.Items.Add(created);
                result.ItemsCreated++;
                continue;
            }

            matchedItemIds.Add(localItem.Id);
            localItem.ExternalId ??= remoteItem.Id;

            var differs =
                localItem.Name != remoteItem.Description
                || localItem.IsComplete != remoteItem.Completed;

            if (remoteItem.UpdatedAt > localItem.UpdatedAt && differs)
            {
                localItem.Name = remoteItem.Description;
                localItem.IsComplete = remoteItem.Completed;
                result.ItemsUpdated++;
            }
        }

        // Items previously synced (have an external id) but missing remotely were deleted.
        foreach (var localItem in localItems)
        {
            if (localItem.ExternalId is not null && !matchedItemIds.Contains(localItem.Id))
            {
                _context.TodoListItems.Remove(localItem);
                result.ItemsDeleted++;
            }
        }
    }

    private static TodoListItem? ResolveLocalItem(
        ExternalTodoItem remoteItem,
        IReadOnlyDictionary<long, TodoListItem> localById,
        IReadOnlyDictionary<string, TodoListItem> localByExternalId
    )
    {
        if (
            TryParseId(remoteItem.SourceId, out var sourceId)
            && localById.TryGetValue(sourceId, out var bySource)
        )
        {
            return bySource;
        }

        return localByExternalId.TryGetValue(remoteItem.Id, out var byExternal) ? byExternal : null;
    }

    /// <summary>
    /// Mirrors a never-synced local list (and its items) to the external API and stores
    /// the returned external ids for future correlation.
    /// </summary>
    private async Task PushNewListAsync(TodoList local, CancellationToken cancellationToken)
    {
        var created = await _client.CreateTodoListAsync(local.ToCreateBody(), cancellationToken);
        ApplyExternalIds(local, created);

        _logger.LogDebug(
            "Mirrored local list {TodoListId} to external {ExternalId}.",
            local.Id,
            created.Id
        );
    }

    /// <summary>
    /// When a synced list has gained new local items (no external id), the external API
    /// can only learn about them via list creation, so delete and recreate the list and
    /// refresh all external ids. Skipped entirely when there is nothing new (minimize calls).
    /// </summary>
    private async Task PushNewItemsViaRecreateAsync(
        TodoList local,
        SyncResult result,
        CancellationToken cancellationToken
    )
    {
        if (local.ExternalId is null || local.Items.All(i => i.ExternalId is not null))
        {
            return;
        }

        var previousExternalId = local.ExternalId;
        await _client.DeleteTodoListAsync(previousExternalId, cancellationToken);

        var recreated = await _client.CreateTodoListAsync(local.ToCreateBody(), cancellationToken);
        ApplyExternalIds(local, recreated);

        _logger.LogDebug(
            "Recreated external list {OldExternalId} -> {NewExternalId} to register new items.",
            previousExternalId,
            recreated.Id
        );
    }

    private static void ApplyExternalIds(TodoList local, ExternalTodoList external)
    {
        local.ExternalId = external.Id;

        var externalItemsBySource = external
            .Items.Where(i => TryParseId(i.SourceId, out _))
            .ToDictionary(i => long.Parse(i.SourceId!, CultureInfo.InvariantCulture));

        foreach (var item in local.Items)
        {
            if (externalItemsBySource.TryGetValue(item.Id, out var externalItem))
            {
                item.ExternalId = externalItem.Id;
            }
        }
    }

    private static bool TryParseId(string? value, out long id)
    {
        id = 0;
        return value is not null
            && long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
    }
}
