using System.Text.Json.Serialization;

namespace TodoApi.Models;

public class TodoList
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public virtual ICollection<TodoItem>? Items { get; set; }
}
