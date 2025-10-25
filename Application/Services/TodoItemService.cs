using AutoMapper;
using TodoApi.Application.Dtos;
using TodoApi.Application.Interfaces;
using TodoApi.Dtos;
using TodoApi.Infrastructure.Interfaces;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Aplication.Services
{
    public class TodoItemService : ITodoItemService
    {
        private ITodoItemRepository _itemRepository;
        private readonly IMapper _mapper;
        public TodoItemService(ITodoItemRepository itemRepository, IMapper mapper)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TodoItemDto>?> GetAllItems(long listId)
        {
            var list = await _itemRepository.GetAllByListIdAsync(listId);

            return list.Any() ? _mapper.Map<IEnumerable<TodoItemDto>>(list) : null;            
        }

        public async Task<TodoItemDto?> GetItem(long listId, int id)
        {
            var item = await _itemRepository.GetByIdAndListIdAsync(id, listId);
            return item != null ? _mapper.Map<TodoItemDto>(item) : null;
        }

        public async Task<OperationResultDto> CreateItem(long listId, CreateItemDto createRequest)
        {
            var itemExits = await _itemRepository.ExitstenceByNameAndListIdAsync(listId, createRequest.Name);
            if (itemExits)
            {
                return new OperationResultDto {  Success  = false , Message = "Item already create"};
            }
            var item = _mapper.Map<TodoItem>(createRequest);
            item.TodoListId = listId;
            var itemId = await _itemRepository.Create(item);
            return new OperationResultDto { Success = true, Message = $"Iteam created with the id {itemId}" };
        }

        public async Task<OperationResultDto> DeleteItem(long listId, int id)
        {
            var item = await _itemRepository.GetByIdAndListIdAsync(id, listId);
            if (item == null)
            {
                return new OperationResultDto { Success = false, Message = "Non-existent Item" };
            }

            await _itemRepository.Delete(id);
            return new OperationResultDto { Success = true };
        }

        public async Task<OperationResultDto> UpdateItem(long listId, int id, UpdateItemDto updateRequest)
        {
            var item = await _itemRepository.GetByIdAndListIdAsync(id, listId);
            if (item == null)
            {
                return new OperationResultDto { Success = false, Message = "Non-existent Item" };
            }
            item.Name = updateRequest.Name ?? item.Name;
            item.Complete = updateRequest.Complete ?? item.Complete;

            await _itemRepository.Update(item);

            return new OperationResultDto { Success = true };
        }
    }
}
