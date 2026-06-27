using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Dtos;
using Todo.Application.Common.Interfaces;

namespace Todo.Application.TodoListItems.Queries.GetTodoListItems;

public class GetTodoListItemsQueryHandler
    : IRequestHandler<GetTodoListItemsQuery, List<TodoListItemDto>?>
{
    private readonly IApplicationDbContext _context;

    public GetTodoListItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TodoListItemDto>?> Handle(
        GetTodoListItemsQuery request,
        CancellationToken cancellationToken
    )
    {
        var todoListExists = await _context.TodoLists.AnyAsync(
            list => list.Id == request.TodoListId,
            cancellationToken
        );

        if (!todoListExists)
        {
            return null;
        }

        return await _context
            .TodoListItems.Where(item => item.TodoListId == request.TodoListId)
            .Select(item => new TodoListItemDto
            {
                Id = item.Id,
                Name = item.Name,
                IsComplete = item.IsComplete,
                TodoListId = item.TodoListId,
            })
            .ToListAsync(cancellationToken);
    }
}
