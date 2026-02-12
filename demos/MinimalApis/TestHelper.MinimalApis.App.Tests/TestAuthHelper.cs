using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using TestHelper.MinimalApis.App.Auth;
using TestHelper.MinimalApis.App.Data;
using TestHelper.MinimalApis.App.Identity;
using TestHelper.MinimalApis.App.Models;

namespace TestHelper.MinimalApis.App.Tests;

public class TestAuthHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IdentityContext _setupContext;
    private readonly AppDbContext _setupAppDbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtGenerator _jwtGenerator;

    public TestAuthHelper(
        IServiceProvider serviceProvider,
        IdentityContext setupContext,
        AppDbContext setupAppDbContext)
    {
        _serviceProvider = serviceProvider;
        _setupContext = setupContext;
        _setupAppDbContext = setupAppDbContext;

        _userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser, IdentityRole, IdentityContext>(
                setupContext,
                serviceProvider.GetRequiredService<IdentityErrorDescriber>()),
            serviceProvider.GetRequiredService<IOptions<IdentityOptions>>(),
            serviceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>(),
            serviceProvider.GetRequiredService<IEnumerable<IUserValidator<ApplicationUser>>>(),
            serviceProvider.GetRequiredService<IEnumerable<IPasswordValidator<ApplicationUser>>>(),
            serviceProvider.GetRequiredService<ILookupNormalizer>(),
            serviceProvider.GetRequiredService<IdentityErrorDescriber>(),
            serviceProvider,
            serviceProvider.GetRequiredService<ILogger<UserManager<ApplicationUser>>>());

        _jwtGenerator = serviceProvider.GetRequiredService<IJwtGenerator>();
    }

    public async Task<(User User, ApplicationUser AspNetUser)> CreateUserAsync(string email, string password = "TheBestPassword1!")
    {
        var aspnetUser = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(aspnetUser, password);
        result.Succeeded.ShouldBeTrue();

        var user = new User { AspNetUserId = aspnetUser.Id };
        await _setupAppDbContext.Users.AddAsync(user);
        await _setupAppDbContext.SaveChangesAsync();

        return (user, aspnetUser);
    }

    public string GenerateJwtToken(User user, ApplicationUser aspNetUser)
    {
        var token = _jwtGenerator.GenerateJwt(aspNetUser, user);
        return token;
    }
}