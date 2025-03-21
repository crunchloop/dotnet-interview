namespace TodoApi.Models
{
    public class Item
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
        public int? TodoListId { get; set; }
        public TodoList? TodoList { get; set; }
    }
}
