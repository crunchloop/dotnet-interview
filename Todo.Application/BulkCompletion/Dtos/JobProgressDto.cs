namespace Todo.Application.BulkCompletion.Dtos;

/// <summary>
/// Snapshot of a job's state, returned by the GET progress endpoint.
/// </summary>
public class JobProgressDto
{
    public Guid JobId { get; set; }
    public long TodoListId { get; set; }
    public required string Status { get; set; }
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
    public double Percentage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
}
