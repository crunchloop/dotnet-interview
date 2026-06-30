using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Exceptions;
using Todo.Application.Common.Interfaces;
using Todo.Application.Sync.Notifications;
using Todo.Domain.Entities;

namespace Todo.Application.TodoListItems.Commands.DeleteTodoListItem;

public class DeleteTodoListItemCommandHandler : IRequestHandler<DeleteTodoListItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public DeleteTodoListItemCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(DeleteTodoListItemCommand request, CancellationToken cancellationToken)
    {
        var todoListItem = await _context
            .TodoListItems.Include(item => item.TodoList)
            .FirstOrDefaultAsync(
                item => item.Id == request.Id && item.TodoListId == request.TodoListId,
                cancellationToken
            );

        if (todoListItem == null)
        {
            throw new NotFoundException(nameof(TodoListItem), request.Id);
        }

        // Capture external ids before removal so the outbound handler can address the
        // remote item (nested under its list).
        var listExternalId = todoListItem.TodoList?.ExternalId;
        var itemExternalId = todoListItem.ExternalId;

        _context.TodoListItems.Remove(todoListItem);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new TodoListItemDeletedNotification(listExternalId, itemExternalId),
            cancellationToken
        );
    }
}
