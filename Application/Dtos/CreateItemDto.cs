using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos
{
    public class CreateItemDto
    {
        [Required]
        public string Name { get; set; }
    }
}
