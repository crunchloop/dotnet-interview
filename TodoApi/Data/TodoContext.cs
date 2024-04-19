using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
    {
    }

    public DbSet<TodoList> TodoList { get; set; } = default!;
    public DbSet<TodoItem> TodoItems { get; set; } = default!;
}
