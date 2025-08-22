using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos;

public class CreateTodoList
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
}
