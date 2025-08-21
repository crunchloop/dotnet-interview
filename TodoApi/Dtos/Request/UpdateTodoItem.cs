namespace TodoApi.Dtos.Request
{
    public class UpdateTodoItem
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public bool? IsComplete { get; set; } = null;
        public long TodoListId { get; set; }
    }
}