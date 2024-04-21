using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {

        modelBuilder.Entity<TodoList>(entity => {

            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Items)
                .WithOne(i => i.TodoList)
                .HasForeignKey(i => i.IdTodoList);

        });

        modelBuilder.Entity<TodoItem>().HasKey(e => e.Id);

    }

    public DbSet<TodoList> TodoList { get; set; } = default!;
    public DbSet<TodoItem> TodoItems { get; set; } = default!;
}
