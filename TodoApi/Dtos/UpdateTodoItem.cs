namespace TodoApi.Dtos
{
    public class UpdateTodoItem
    {
        public required string Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
