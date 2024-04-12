namespace TodoApi.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        public string ItemName { get; set; }

        public virtual TodoList TodoList { get; set; }

        public int TodoListId { get; set; } 
    }
}
