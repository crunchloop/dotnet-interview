namespace TodoApi.Application.Dtos
{
    public class TodoItemDto
    {
        public int Id { get; set; }
        public int TodoListId { get; set; }
        public string Name { get; set; }
        public bool Complete { get; set; }

    }
}
