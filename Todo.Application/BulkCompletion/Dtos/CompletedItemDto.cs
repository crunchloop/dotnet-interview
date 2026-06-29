namespace Todo.Application.BulkCompletion.Dtos;

/// <summary>
/// A single item that was just completed by a bulk-completion job, shaped so the
/// frontend can render it immediately. The entity only carries <c>Name</c> and
/// <c>IsComplete</c>; <see cref="CompletedAt"/> is the processing timestamp and is
/// not persisted.
/// </summary>
public class CompletedItemDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CompletedAt { get; set; }
}
