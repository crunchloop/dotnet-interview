namespace Todo.Application.Common.Dtos;

public class TodoListItemDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
    public long TodoListId { get; set; }
}
