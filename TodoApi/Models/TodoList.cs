namespace TodoApi.Models;

public class TodoList
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public IList<TodoListItem> Items { get; set; } = new List<TodoListItem>();
}
