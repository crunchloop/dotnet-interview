using System.Text.Json.Serialization;

namespace TodoApi.Models
{
    public class TodoItem
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool Completed { get; set; }
        [JsonIgnore]
        public long IdTodoList { get; set; }
        [JsonIgnore]
        public virtual TodoList? TodoList { get; set; }
    }
}
 