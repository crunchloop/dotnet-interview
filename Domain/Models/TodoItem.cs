using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class TodoItem
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public long TodoListId { get; set; }

        [Required]
        public string Name { get; set; }
        public bool Complete { get; set; }
    }
}
