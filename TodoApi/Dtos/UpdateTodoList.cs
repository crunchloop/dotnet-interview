namespace TodoApi.Dtos;

public class UpdateTodoList
{
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
    public long TodoListId { get; set; }
}
