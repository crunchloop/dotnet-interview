using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Exceptions;
using Todo.Application.Common.Interfaces;
using Todo.Domain.Entities;

namespace Todo.Application.TodoListItems.Commands.UpdateTodoListItem;

public class UpdateTodoListItemCommandHandler : IRequestHandler<UpdateTodoListItemCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTodoListItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTodoListItemCommand request, CancellationToken cancellationToken)
    {
        var todoListItem = await _context.TodoListItems.FirstOrDefaultAsync(
            item => item.Id == request.Id && item.TodoListId == request.TodoListId,
            cancellationToken
        );

        if (todoListItem == null)
        {
            throw new NotFoundException(nameof(TodoListItem), request.Id);
        }

        todoListItem.Name = request.Name;
        todoListItem.IsComplete = request.IsComplete;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
