using Todo.Domain.Common;

namespace Todo.Domain.Entities;

public class TodoList : IAuditableEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }

    /// <summary>
    /// Identifier assigned by the external API once this list has been synced.
    /// Null until the first successful outbound/inbound sync correlates the record.
    /// </summary>
    public string? ExternalId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public IList<TodoListItem> Items { get; set; } = new List<TodoListItem>();
}
