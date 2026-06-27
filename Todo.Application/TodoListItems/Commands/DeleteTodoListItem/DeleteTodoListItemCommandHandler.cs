using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Exceptions;
using Todo.Application.Common.Interfaces;
using Todo.Domain.Entities;

namespace Todo.Application.TodoListItems.Commands.DeleteTodoListItem;

public class DeleteTodoListItemCommandHandler : IRequestHandler<DeleteTodoListItemCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTodoListItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTodoListItemCommand request, CancellationToken cancellationToken)
    {
        var todoListItem = await _context.TodoListItems.FirstOrDefaultAsync(
            item => item.Id == request.Id && item.TodoListId == request.TodoListId,
            cancellationToken
        );

        if (todoListItem == null)
        {
            throw new NotFoundException(nameof(TodoListItem), request.Id);
        }

        _context.TodoListItems.Remove(todoListItem);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
