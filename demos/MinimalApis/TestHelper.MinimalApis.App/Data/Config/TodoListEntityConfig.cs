using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestHelper.MinimalApis.App.Models;

namespace TestHelper.MinimalApis.App.Data.Config;

public class TodoListEntityConfig : IEntityTypeConfiguration<TodoList>
{
    public void Configure(EntityTypeBuilder<TodoList> builder)
    {
        builder.ToTable("TodoLists");

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .IsRequired();

        builder.HasMany(t => t.Items)
            .WithOne()
            .HasForeignKey(t => t.TodoListId);
    }
}