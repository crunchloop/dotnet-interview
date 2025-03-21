namespace TodoApi.Models;

public class TodoList
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public List<Item> Items { get; set; }

    public TodoList()
    {
        Items = new List<Item>();
    }
}
