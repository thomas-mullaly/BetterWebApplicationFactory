using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using TestHelper.MinimalApis.App.Data;
using TestHelper.MinimalApis.App.Identity;

namespace TestHelper.MinimalApis.App.Tests;

[CollectionDefinition(nameof(EndpointTestCollectionDefinition))]
public class EndpointTestCollectionDefinition : ICollectionFixture<EndpointTestFixture>;

[Collection(nameof(EndpointTestCollectionDefinition))]
public abstract class EndpointTestBase : AspnetTestBase<ClientFactory<Program>, Program>, IAsyncLifetime
{
    protected AppDbContext SetupDbContext { get; private set; } = default!;
    protected AppDbContext AssertionDbContext { get; private set; } = default!;
    protected AppDbContext TestDbContext { get; private set; } = default!;
    protected IdentityContext SetupIdentityContext { get; private set; } = default!;
    protected IdentityContext AssertionIdentityContext { get; private set; } = default!;
    protected IdentityContext TestIdentityContext { get; private set; } = default!;
    protected UserManager<ApplicationUser> UserManager { get; private set; } = default!;

    protected override ClientFactory<Program> ClientFactory
    {
        get
        {
            return new ClientFactory<Program>(TestId, WebAppFactory)
                .AddServiceOverride(services =>
                {
                    services.AddSingleton(TestDbContext);
                    services.AddSingleton(TestIdentityContext);
                });
        }
    }

    protected EndpointTestFixture Fixture { get; }

    protected EndpointTestBase(EndpointTestFixture fixture) : base(fixture.Factory)
    {
        Fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        var appDbConnection = new NpgsqlConnection(Fixture.TestContainerFixture.AppConnectionString);
        var appDbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(appDbConnection)
            .UseInternalServiceProvider(Fixture.TestContainerFixture.EfServiceProvider)
            .Options;

        SetupDbContext = new AppDbContext(appDbContextOptions);
        TestDbContext = new AppDbContext(appDbContextOptions);
        AssertionDbContext = new AppDbContext(appDbContextOptions);

        var identityDbConnection = new NpgsqlConnection(Fixture.TestContainerFixture.IdentityConnectionString);
        var identityDbContextOptions = new DbContextOptionsBuilder<IdentityContext>()
            .UseNpgsql(identityDbConnection)
            .UseInternalServiceProvider(Fixture.TestContainerFixture.EfServiceProvider)
            .Options;

        SetupIdentityContext = new IdentityContext(identityDbContextOptions);
        AssertionIdentityContext = new IdentityContext(identityDbContextOptions);
        TestIdentityContext = new IdentityContext(identityDbContextOptions);

        await SetupAndEnrollContextsInTransaction(SetupDbContext, AssertionDbContext, TestDbContext);
        await SetupAndEnrollContextsInTransaction(SetupIdentityContext, AssertionIdentityContext, TestIdentityContext);

        await using var scope = WebAppFactory.Services.CreateAsyncScope();
        UserManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser, IdentityRole, IdentityContext>(
                SetupIdentityContext,
                scope.ServiceProvider.GetRequiredService<IdentityErrorDescriber>()),
            scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>(),
            scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<IEnumerable<IUserValidator<ApplicationUser>>>(),
            scope.ServiceProvider.GetRequiredService<IEnumerable<IPasswordValidator<ApplicationUser>>>(),
            scope.ServiceProvider.GetRequiredService<ILookupNormalizer>(),
            scope.ServiceProvider.GetRequiredService<IdentityErrorDescriber>(),
            scope.ServiceProvider,
            scope.ServiceProvider.GetRequiredService<ILogger<UserManager<ApplicationUser>>>());

        ClientFactory.AddServiceOverride(services =>
        {
            services.AddSingleton(TestDbContext);
            services.AddSingleton(TestIdentityContext);
        });
    }

    public async ValueTask DisposeAsync()
    {
        await SetupDbContext.Database.RollbackTransactionAsync();
        await SetupIdentityContext.Database.RollbackTransactionAsync();

        await SetupDbContext.DisposeAsync();
        await AssertionDbContext.DisposeAsync();
        await TestDbContext.DisposeAsync();
        await SetupIdentityContext.DisposeAsync();
        await AssertionIdentityContext.DisposeAsync();
        await TestIdentityContext.DisposeAsync();
    }

    private async Task SetupAndEnrollContextsInTransaction(DbContext primary, params DbContext[] secondaryContexts)
    {
        var transaction = await primary.Database.BeginTransactionAsync();

        foreach (var otherDbContext in secondaryContexts)
        {
            await otherDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());
        }
    }
}