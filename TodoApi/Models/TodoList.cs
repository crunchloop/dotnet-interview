namespace TodoApi.Models;

public class TodoList
{
    public long Id { get; set; }
    public required string Name { get; set; }

    // Navigation property --> relación one-to-many con TodoItem comienza con una colección vacía
    public IList<TodoItem> Items { get; set; } = new List<TodoItem>();
}
