using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos
{
    public class UpdateTodoItem
    {
        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }
        public bool IsCompleted { get; set; }
    }
}
