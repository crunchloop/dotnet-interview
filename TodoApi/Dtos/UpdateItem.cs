namespace TodoApi.Dtos
{
    public class UpdateItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
