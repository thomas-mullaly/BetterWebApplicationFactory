using Microsoft.EntityFrameworkCore;
using TestHelper.MinimalApis.App.Models;

namespace TestHelper.MinimalApis.App.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<TodoList> TodoLists { get; set; } = default!;
    public DbSet<TodoItem> TodoItems { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
}