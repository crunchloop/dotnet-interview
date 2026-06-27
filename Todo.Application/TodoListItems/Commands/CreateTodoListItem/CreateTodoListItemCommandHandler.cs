using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Exceptions;
using Todo.Application.Common.Interfaces;
using Todo.Domain.Entities;

namespace Todo.Application.TodoListItems.Commands.CreateTodoListItem;

public class CreateTodoListItemCommandHandler : IRequestHandler<CreateTodoListItemCommand, long>
{
    private readonly IApplicationDbContext _context;

    public CreateTodoListItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<long> Handle(
        CreateTodoListItemCommand request,
        CancellationToken cancellationToken
    )
    {
        var todoListExists = await _context.TodoLists.AnyAsync(
            list => list.Id == request.TodoListId,
            cancellationToken
        );

        if (!todoListExists)
        {
            throw new NotFoundException(nameof(TodoList), request.TodoListId);
        }

        var todoListItem = new TodoListItem
        {
            Name = request.Name,
            IsComplete = request.IsComplete,
            TodoListId = request.TodoListId,
        };

        _context.TodoListItems.Add(todoListItem);
        await _context.SaveChangesAsync(cancellationToken);

        return todoListItem.Id;
    }
}
