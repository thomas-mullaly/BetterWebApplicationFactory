namespace TestHelper.MinimalApis.App.Models;

public class TodoItem
{
    public Guid Id { get; set; }
    public Guid TodoListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsComplete => CompletedAt.HasValue;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}