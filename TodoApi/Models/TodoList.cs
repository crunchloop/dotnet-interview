namespace TodoApi.Models;

public class TodoList
{
    public long Id { get; set; }
    public string? Name { get; set; }

    public IList<TodoItem> Tasks { get; set; }
}