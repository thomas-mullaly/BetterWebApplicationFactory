using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestHelper.MinimalApis.App.Models;

namespace TestHelper.MinimalApis.App.Data.Config;

public class TodoItemEntityConfig : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("TodoItems");
    }
}