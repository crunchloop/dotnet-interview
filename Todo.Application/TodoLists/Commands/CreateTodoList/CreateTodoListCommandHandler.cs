using MediatR;
using Todo.Application.Common.Interfaces;
using Todo.Application.Sync.Notifications;
using Todo.Domain.Entities;

namespace Todo.Application.TodoLists.Commands.CreateTodoList;

public class CreateTodoListCommandHandler : IRequestHandler<CreateTodoListCommand, long>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public CreateTodoListCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<long> Handle(
        CreateTodoListCommand request,
        CancellationToken cancellationToken
    )
    {
        var todoList = new TodoList { Name = request.Name };

        _context.TodoLists.Add(todoList);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new TodoListCreatedNotification(todoList.Id), cancellationToken);

        return todoList.Id;
    }
}
