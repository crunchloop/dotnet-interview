namespace Todo.Web.Contracts;

public class UpdateTodoListItemRequest
{
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
}
