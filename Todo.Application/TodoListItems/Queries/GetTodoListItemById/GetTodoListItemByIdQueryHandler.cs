using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Dtos;
using Todo.Application.Common.Interfaces;

namespace Todo.Application.TodoListItems.Queries.GetTodoListItemById;

public class GetTodoListItemByIdQueryHandler
    : IRequestHandler<GetTodoListItemByIdQuery, TodoListItemDto?>
{
    private readonly IApplicationDbContext _context;

    public GetTodoListItemByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TodoListItemDto?> Handle(
        GetTodoListItemByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .TodoListItems.Where(item =>
                item.Id == request.Id && item.TodoListId == request.TodoListId
            )
            .Select(item => new TodoListItemDto
            {
                Id = item.Id,
                Name = item.Name,
                IsComplete = item.IsComplete,
                TodoListId = item.TodoListId,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
