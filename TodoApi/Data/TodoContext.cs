using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options) { }

    public DbSet<TodoList> TodoList { get; set; } = default!;
    public DbSet<TodoItem> TodoItem { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // relationships y constraints usando Fluent API (rela´ción bidireccional)
        // para todoList, el borrado se hace en cascada, para que borrer los todoItems asociados

        modelBuilder.Entity<TodoList>(todoList =>
        {
            todoList.HasKey(tl => tl.Id);
            todoList.Property(tl => tl.Name).IsRequired();
            // relationship
            todoList
                .HasMany(tl => tl.Items)
                .WithOne(ti => ti.TodoList)
                .HasForeignKey(ti => ti.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TodoItem>(todoItem =>
        {
            todoItem.HasKey(ti => ti.Id);
            todoItem.Property(ti => ti.Name).IsRequired();
            todoItem.Property(ti => ti.IsComplete).HasDefaultValue(false);
            todoItem.Property(ti => ti.CreatedAt).IsRequired();
            todoItem.Property(ti => ti.UpdatedAt).IsRequired();
        });
    }
}
