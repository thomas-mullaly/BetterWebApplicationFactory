using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TestHelper.MinimalApis.App.Dtos;
using TestHelper.MinimalApis.App.Dtos.Auth;
using TestHelper.MinimalApis.App.Identity;
using TestHelper.MinimalApis.App.Models;
using TestHelper.MinimalApis.App.Requests.Auth;

namespace TestHelper.MinimalApis.App.Tests.Auth;

public class LoginTests : EndpointTestBase
{
    public LoginTests(EndpointTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task IfTheUserDoesNotExist_ReturnsUnauthorized()
    {
        var client = ClientFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/login", new LoginRequest
        {
            Username = "nonexistentuser@example.com",
            Password = "password123"
        }, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IfTheUserExists_ButThePasswordDoesNotMatch_ReturnsUnauthorized()
    {
        _ = await AuthHelper.CreateUserAsync("test@example.com", "TheBestPassword1!");

        var client = ClientFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/login", new LoginRequest
        {
            Username = "test@example",
            Password = "wrongpassword"
        }, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IfTheUserExists_AndThePasswordMatches_ReturnsOk()
    {
        _ = await AuthHelper.CreateUserAsync("test@example.com", "TheBestPassword1!");

        var client = ClientFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/login", new LoginRequest
        {
            Username = "test@example",
            Password = "TheBestPassword1!"
        }, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var apiResult = await response.Content.ReadFromJsonAsync<ApiResult<LoginDto>>(TestContext.Current.CancellationToken);

        apiResult.IsSuccess.ShouldBeTrue();
        apiResult.Data.Token.ShouldNotBeNullOrWhiteSpace();
    }
}