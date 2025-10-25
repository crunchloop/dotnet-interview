using AutoMapper;
using TodoApi.Application.Dtos;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Application.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TodoItem, TodoItemDto>().ReverseMap();
            CreateMap<TodoItem, CreateItemDto>().ReverseMap();

        }
    }
}
