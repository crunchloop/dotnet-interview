using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todo.Domain.Entities;

namespace Todo.Persistence.Configurations;

public class TodoListItemConfiguration : IEntityTypeConfiguration<TodoListItem>
{
    public void Configure(EntityTypeBuilder<TodoListItem> builder)
    {
        builder.Property(item => item.Name).IsRequired();

        builder.Property(item => item.ExternalId).HasMaxLength(100);
        builder.HasIndex(item => item.ExternalId);

        builder
            .HasOne(item => item.TodoList)
            .WithMany(list => list.Items)
            .HasForeignKey(item => item.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
