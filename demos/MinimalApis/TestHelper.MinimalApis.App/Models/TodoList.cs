namespace TestHelper.MinimalApis.App.Models;

public class TodoList
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public User CreatedBy { get; set; } = default!;

    public List<TodoItem> Items { get; set; } = new();
}