namespace TodoApi.Dtos;

public class CreateTodoListItem
{
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
}
