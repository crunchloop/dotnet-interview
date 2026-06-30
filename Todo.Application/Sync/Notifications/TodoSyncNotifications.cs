using MediatR;

namespace Todo.Application.Sync.Notifications;

/// <summary>
/// Domain notifications published by the local command handlers after a successful
/// <c>SaveChanges</c>. Dedicated notification handlers translate them into outbound
/// calls against the external API, keeping the command handlers thin and the
/// integration decoupled (and optional — a missing external API never breaks a local write).
/// </summary>
public record TodoListCreatedNotification(long TodoListId) : INotification;

public record TodoListUpdatedNotification(long TodoListId) : INotification;

/// <summary>
/// Carries the already-resolved external id because the local row is gone by the time
/// the notification is handled.
/// </summary>
public record TodoListDeletedNotification(string? ExternalId) : INotification;

public record TodoListItemUpdatedNotification(long TodoListId, long TodoListItemId) : INotification;

public record TodoListItemDeletedNotification(string? ListExternalId, string? ItemExternalId)
    : INotification;
