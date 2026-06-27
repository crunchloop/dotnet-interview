namespace Todo.Web.Contracts;

public class CreateTodoListItemRequest
{
    public required string Name { get; set; }
    public bool IsComplete { get; set; }
}
