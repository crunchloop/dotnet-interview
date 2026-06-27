using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Dtos;
using Todo.Application.Common.Interfaces;

namespace Todo.Application.TodoLists.Queries.GetTodoLists;

public class GetTodoListsQueryHandler : IRequestHandler<GetTodoListsQuery, List<TodoListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTodoListsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TodoListDto>> Handle(
        GetTodoListsQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .TodoLists.Select(list => new TodoListDto { Id = list.Id, Name = list.Name })
            .ToListAsync(cancellationToken);
    }
}
