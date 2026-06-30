namespace Todo.Application.Sync.Models;

/// <summary>
/// Summary of a sync run. The created/updated/deleted counters reflect changes applied
/// to the LOCAL store during inbound reconciliation. Outbound pushes (mirroring local
/// lists/items to the external API) are logged rather than counted here. Per-entity
/// failures are collected in <see cref="Errors"/> so one bad record never aborts the run.
/// </summary>
public class SyncResult
{
    public int ListsCreated { get; set; }
    public int ListsUpdated { get; set; }
    public int ListsDeleted { get; set; }
    public int ItemsCreated { get; set; }
    public int ItemsUpdated { get; set; }
    public int ItemsDeleted { get; set; }

    public List<string> Errors { get; } = new();

    public bool HasErrors => Errors.Count > 0;
}
