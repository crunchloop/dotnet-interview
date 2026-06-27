using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Exceptions;
using Todo.Application.Common.Interfaces;
using Todo.Domain.Entities;

namespace Todo.Application.TodoLists.Commands.DeleteTodoList;

public class DeleteTodoListCommandHandler : IRequestHandler<DeleteTodoListCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTodoListCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTodoListCommand request, CancellationToken cancellationToken)
    {
        var todoList = await _context.TodoLists.FirstOrDefaultAsync(
            list => list.Id == request.Id,
            cancellationToken
        );

        if (todoList == null)
        {
            throw new NotFoundException(nameof(TodoList), request.Id);
        }

        _context.TodoLists.Remove(todoList);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
