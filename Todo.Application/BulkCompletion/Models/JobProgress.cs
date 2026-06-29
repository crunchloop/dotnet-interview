namespace Todo.Application.BulkCompletion.Models;

/// <summary>
/// In-memory tracking record for a bulk-completion job. A single background task
/// mutates an instance, so the fields are intentionally simple value types.
/// </summary>
public class JobProgress
{
    public Guid JobId { get; set; }
    public long TodoListId { get; set; }
    public JobStatus Status { get; set; }
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }

    public double Percentage =>
        TotalCount == 0 ? 100 : Math.Round((double)CompletedCount / TotalCount * 100, 2);
}
