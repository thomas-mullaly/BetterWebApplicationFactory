using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TestHelper.MinimalApis.App.Auth;

namespace TestHelper.MinimalApis.App.Middleware;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CurrentUser currentUser)
    {
        if (context.User.Identity?.IsAuthenticated is true)
        {
            currentUser.UserId = Guid.Parse(context.User.Identity.Name);
            currentUser.Username = context.User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        }

        await _next(context);
    }
}