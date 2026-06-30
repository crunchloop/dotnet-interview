namespace Todo.Domain.Common;

/// <summary>
/// Implemented by entities whose <see cref="CreatedAt"/> / <see cref="UpdatedAt"/>
/// timestamps are maintained automatically on save. Timestamps drive the
/// newer-wins reconciliation performed by the external sync.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
