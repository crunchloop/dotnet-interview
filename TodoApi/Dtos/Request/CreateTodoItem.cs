namespace TodoApi.Dtos.Request
{
    public class CreateTodoItem
    {
        public required string Name { get; set; }
        public bool IsComplete { get; set; } = false;
        public long TodoListId { get; set; }
    }
}
