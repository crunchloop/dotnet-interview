namespace TodoApi.Models
{
    public class TodoItem
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public bool IsComplete { get; set; } = false;

        // campos de auditoria
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property y foreign key --> relación many-to-one con TodoList
        public long TodoListId { get; set; }
        public TodoList? TodoList { get; set; }
    }
}
