using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using TestHelper.MinimalApis.App.Data;
using TestHelper.MinimalApis.App.Identity;
using TestHelper.MinimalApis.App.Tests;

[assembly: AssemblyFixture(typeof(TestContainerFixture))]

namespace TestHelper.MinimalApis.App.Tests;

public class TestContainerFixture : IAsyncLifetime
{
    public PostgreSqlContainer TestContainer { get; }
    public PostgreSqlContainer IdentityContainer { get; }
    public IServiceProvider EfServiceProvider { get; }

    public string IdentityConnectionString { get; private set; } = string.Empty;
    public string AppConnectionString { get; private set; } = string.Empty;

    public TestContainerFixture()
    {
        var services = new ServiceCollection();
        services.AddEntityFrameworkNpgsql();

        EfServiceProvider = services.BuildServiceProvider();

        TestContainer = new PostgreSqlBuilder("postgres:latest")
            .WithDatabase("App")
            .WithPassword("realgoodpassword")
            .WithCleanUp(true)
            .Build();

        IdentityContainer = new PostgreSqlBuilder("postgres:latest")
            .WithDatabase("Identity")
            .WithPassword("realgoodpassword")
            .WithCleanUp(true)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await TestContainer.StartAsync();
        await IdentityContainer.StartAsync();

        IdentityConnectionString = IdentityContainer.GetConnectionString();

        var identityContextOptions = new DbContextOptionsBuilder<IdentityContext>()
            .UseNpgsql(IdentityConnectionString)
            .UseInternalServiceProvider(EfServiceProvider)
            .Options;

        await using var identityDbContext = new IdentityContext(identityContextOptions);
        await identityDbContext.Database.EnsureCreatedAsync();

        AppConnectionString = TestContainer.GetConnectionString();

        var appContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(AppConnectionString)
            .UseInternalServiceProvider(EfServiceProvider)
            .Options;

        await using var appDbContext = new AppDbContext(appContextOptions);
        await appDbContext.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await TestContainer.DisposeAsync();
        await IdentityContainer.DisposeAsync();
    }
}