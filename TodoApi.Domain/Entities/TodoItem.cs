namespace TodoApi.Models
{
    public class TodoItem
    {
        public TodoItem()
        {
            
        }

        public TodoItem(string itemName, int todoListId)
        {
            this.ItemName = itemName;
            this.TodoListId = todoListId;
        }

        public int Id { get; set; }

        public string ItemName { get; set; }

        public virtual TodoList TodoList { get; set; }

        public int TodoListId { get; set; } 
    }
}
