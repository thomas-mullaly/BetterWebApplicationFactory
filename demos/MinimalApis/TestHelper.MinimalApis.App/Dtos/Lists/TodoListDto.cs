namespace TestHelper.MinimalApis.App.Dtos.Lists;

public class TodoListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TodoItemDto> Items { get; set; } = new List<TodoItemDto>();
}