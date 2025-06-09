namespace TodoApi.Dtos;

public class UpdateTodoItem
{
    public string Description { get; set; } = default!;
    public bool IsCompleted { get; set; }
}
