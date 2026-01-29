namespace TestHelper.MinimalApis.App.Dtos.Lists;

public class TodoItemDto
{
    public Guid Id { get; set; }
    public Guid TodoListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsComplete => CompletedAt.HasValue;
}