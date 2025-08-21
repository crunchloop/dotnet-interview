namespace TodoApi.Dtos.Response
{
    public class TodoItemResponseDto
    {
        public TodoItemResponseDto(){}
        public TodoItemResponseDto(long id, string name, bool isComplete, long todoListId)
        {
            this.Id = id;
            this.Name = name;
            this.IsComplete = isComplete;
            this.TodoListId = todoListId;
        }
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; } = false;
        public long TodoListId { get; set; }
    }
}
