namespace TodoApi.Models;

public class TodoList
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public IList<TodoItem> Items { get; set; } = new List<TodoItem>();
}
