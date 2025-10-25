
using TodoApi.Application.Dtos;
using TodoApi.Dtos;

namespace TodoApi.Application.Interfaces
{
    public interface ITodoItemService
    {
        Task<OperationResultDto> CreateItem(long listId, CreateItemDto createRequest);
        Task<OperationResultDto> DeleteItem(long Id, int id);
        Task<IEnumerable<TodoItemDto>?> GetAllItems(long listId);
        Task<TodoItemDto?> GetItem(long listId, int id);
        Task<OperationResultDto> UpdateItem(long listId, int id, UpdateItemDto updateRequest);
    }
}
