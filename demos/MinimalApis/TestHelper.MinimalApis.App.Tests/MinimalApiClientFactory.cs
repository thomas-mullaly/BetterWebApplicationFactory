using TestHelper.MinimalApis.App.Identity;
using TestHelper.MinimalApis.App.Models;

namespace TestHelper.MinimalApis.App.Tests;

public class MinimalApiClientFactory : ClientFactory<Program, MinimalApiClientFactory>
{
    private readonly TestAuthHelper _authHelper;
    private string? _jwt;

    public MinimalApiClientFactory(
        Guid testId,
        MinimalApiWebApplicationFactory webApplicationFactory,
        TestAuthHelper authHelper)
        : base(testId, webApplicationFactory)
    {
        _authHelper = authHelper;
    }

    public MinimalApiClientFactory AuthenticateAsUser(User user, ApplicationUser aspNetUser)
    {
        _jwt = _authHelper.GenerateJwtToken(user, aspNetUser);
        return this;
    }

    protected override void CustomizeHttpClient(HttpClient client)
    {
        if (_jwt is not null)
        {
            client.DefaultRequestHeaders.Authorization = new("Bearer", _jwt);
        }
    }
}