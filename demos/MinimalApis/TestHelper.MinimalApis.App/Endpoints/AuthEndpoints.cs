using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestHelper.MinimalApis.App.Auth;
using TestHelper.MinimalApis.App.Data;
using TestHelper.MinimalApis.App.Dtos.Auth;
using TestHelper.MinimalApis.App.Helpers;
using TestHelper.MinimalApis.App.Identity;
using TestHelper.MinimalApis.App.Requests.Auth;

namespace TestHelper.MinimalApis.App.Endpoints;

public static class AuthEndpoints
{
    public static void RegisterRoutes(RouteGroupBuilder routeGroup)
    {
        routeGroup.MapPost("/login", Login).AllowAnonymous();
    }

    private static async Task<IResult> Login(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext dbContext,
        IJwtGenerator jwtGenerator,
        LoginRequest request,
        CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Username);
        if (user == null) return ApiResults.Unauthorized();

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded) return ApiResults.Unauthorized();

        var appUser = await dbContext.Users.SingleOrDefaultAsync(u => u.AspNetUserId == user.Id, ct);
        if (appUser == null) return ApiResults.Unauthorized();

        var token = jwtGenerator.GenerateJwt(user, appUser);
        return ApiResults.Ok(new LoginDto { Token = token });
    }
}