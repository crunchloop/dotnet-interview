namespace TodoApi.Application.Interfaces
{
    public interface ITodoListService
    {
        Task<bool> ListExist(long listId);
        Task CompleteList(long listId);
    }
}
