namespace TodoApi.Dtos;

public class UpdateTodoListItem
{
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
}
