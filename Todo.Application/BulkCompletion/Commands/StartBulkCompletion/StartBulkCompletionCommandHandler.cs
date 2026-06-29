using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common.Exceptions;
using Todo.Application.Common.Interfaces;
using Todo.Domain.Entities;

namespace Todo.Application.BulkCompletion.Commands.StartBulkCompletion;

public class StartBulkCompletionCommandHandler : IRequestHandler<StartBulkCompletionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IBulkCompletionService _bulkCompletionService;

    public StartBulkCompletionCommandHandler(
        IApplicationDbContext context,
        IBulkCompletionService bulkCompletionService
    )
    {
        _context = context;
        _bulkCompletionService = bulkCompletionService;
    }

    public async Task<Guid> Handle(
        StartBulkCompletionCommand request,
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

        return _bulkCompletionService.StartJob(request.TodoListId);
    }
}
