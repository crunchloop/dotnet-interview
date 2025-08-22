using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos
{
    public class CreateTodoItem
    {
        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }
    }
}
