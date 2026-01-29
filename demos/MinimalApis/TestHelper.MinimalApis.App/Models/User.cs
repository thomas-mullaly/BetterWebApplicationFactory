namespace TestHelper.MinimalApis.App.Models;

public class User
{
    public Guid Id { get; set; }
    public string AspNetUserId { get; set; } = string.Empty;
}