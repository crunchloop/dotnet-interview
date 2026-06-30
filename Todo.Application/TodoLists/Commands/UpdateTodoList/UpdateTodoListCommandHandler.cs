using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Exceptions;
using Todo.Application.Common.Interfaces;
using Todo.Application.Sync.Notifications;
using Todo.Domain.Entities;

namespace Todo.Application.TodoLists.Commands.UpdateTodoList;

public class UpdateTodoListCommandHandler : IRequestHandler<UpdateTodoListCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public UpdateTodoListCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(UpdateTodoListCommand request, CancellationToken cancellationToken)
    {
        var todoList = await _context.TodoLists.FirstOrDefaultAsync(
            list => list.Id == request.Id,
            cancellationToken
        );

        if (todoList == null)
        {
            throw new NotFoundException(nameof(TodoList), request.Id);
        }

        todoList.Name = request.Name;
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new TodoListUpdatedNotification(todoList.Id), cancellationToken);
    }
}
