using Todo.Domain.Common;

namespace Todo.Domain.Entities;

public class TodoListItem : IAuditableEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
    public long TodoListId { get; set; }
    public TodoList? TodoList { get; set; }

    /// <summary>
    /// Identifier assigned by the external API once this item has been synced.
    /// Null until the first successful sync correlates the record.
    /// </summary>
    public string? ExternalId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
