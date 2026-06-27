using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Dtos;
using Todo.Application.Common.Interfaces;

namespace Todo.Application.TodoLists.Queries.GetTodoListById;

public class GetTodoListByIdQueryHandler : IRequestHandler<GetTodoListByIdQuery, TodoListDto?>
{
    private readonly IApplicationDbContext _context;

    public GetTodoListByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TodoListDto?> Handle(
        GetTodoListByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .TodoLists.Where(list => list.Id == request.Id)
            .Select(list => new TodoListDto { Id = list.Id, Name = list.Name })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
