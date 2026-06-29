namespace Todo.Application.BulkCompletion.Dtos;

/// <summary>
/// Broadcast as the "ProgressUpdate" SignalR message after each processed batch.
/// </summary>
public class ProgressUpdateDto
{
    public Guid JobId { get; set; }
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
    public double Percentage { get; set; }
    public double ItemsPerSecond { get; set; }
    public double EstimatedSecondsRemaining { get; set; }
    public IReadOnlyList<CompletedItemDto> CompletedItems { get; set; } =
        Array.Empty<CompletedItemDto>();
}
