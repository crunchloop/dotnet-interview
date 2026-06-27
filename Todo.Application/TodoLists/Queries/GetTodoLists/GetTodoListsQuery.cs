using MediatR;
using Todo.Application.Common.Dtos;

namespace Todo.Application.TodoLists.Queries.GetTodoLists;

public class GetTodoListsQuery : IRequest<List<TodoListDto>> { }
