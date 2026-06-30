using MediatR;
using Todo.Application.Sync.Models;

namespace Todo.Application.Sync.Commands.RunSync;

/// <summary>
/// Triggers a full bidirectional sync against the external API: fetch-all inbound
/// reconciliation plus outbound mirroring of any local records the external API does
/// not yet know about. Invoked both by the manual <c>POST /api/sync</c> endpoint and the
/// background worker.
/// </summary>
public record RunSyncCommand : IRequest<SyncResult>;
