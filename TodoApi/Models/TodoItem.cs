using System.ComponentModel.DataAnnotations.Schema;
namespace TodoApi.Models;

public class TodoItem
{
    public long Id { get; set; }
    [ForeignKey("TodoList")]
    public long TodoListId { get; set; }
    public required string Description { get; set; }
    public bool IsCompleted { get; set; }
    public TodoList TodoList { get; set; }
}

