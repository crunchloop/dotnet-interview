using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public DbSet<TodoItem> TodoItem { get; set; } 
        public DbSet<TodoList> TodoList { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoItem>()
                .HasOne<TodoList>()
                .WithMany()
                .HasForeignKey(t => t.TodoListId);

            modelBuilder.Entity<TodoItem>();
            modelBuilder.Entity<TodoList>();
        }
    }
}