using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Exceptions;
using Todo.Application.Common.Interfaces;
using Todo.Application.Sync.Notifications;
using Todo.Domain.Entities;

namespace Todo.Application.TodoLists.Commands.DeleteTodoList;

public class DeleteTodoListCommandHandler : IRequestHandler<DeleteTodoListCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public DeleteTodoListCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
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

        // Capture the external id before the row is gone so the outbound handler can
        // propagate the deletion.
        var externalId = todoList.ExternalId;

        _context.TodoLists.Remove(todoList);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new TodoListDeletedNotification(externalId), cancellationToken);
    }
}
