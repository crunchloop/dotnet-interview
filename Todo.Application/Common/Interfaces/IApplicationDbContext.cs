using Microsoft.EntityFrameworkCore;
using Todo.Domain.Entities;

namespace Todo.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoListItem> TodoListItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
