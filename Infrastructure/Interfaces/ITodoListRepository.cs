namespace TodoApi.Infrastructure.Interfaces
{
    public interface ITodoListRepository
    {
        Task<bool> Exits(long listId);
    }
}
