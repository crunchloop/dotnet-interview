namespace TodoApi.Models
{
    public class TodoItem
    {
        public long Id { get; set; }
        public required string Title { get; set; }
        public bool IsCompleted { get; set; }

        public long TodoListId { get; set; }
        public TodoList? TodoList { get; set; }
    }
}
