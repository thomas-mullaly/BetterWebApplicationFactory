namespace TestHelper.MinimalApis.App.Auth;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Username { get; }
}

public class CurrentUser : ICurrentUser
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsAnonymous => UserId == Guid.Empty;
}