using MediatR;
using Todo.Application.Common.Interfaces;
using Todo.Domain.Entities;

namespace Todo.Application.TodoLists.Commands.CreateTodoList;

public class CreateTodoListCommandHandler : IRequestHandler<CreateTodoListCommand, long>
{
    private readonly IApplicationDbContext _context;

    public CreateTodoListCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<long> Handle(
        CreateTodoListCommand request,
        CancellationToken cancellationToken
    )
    {
        var todoList = new TodoList { Name = request.Name };

        _context.TodoLists.Add(todoList);
        await _context.SaveChangesAsync(cancellationToken);

        return todoList.Id;
    }
}
