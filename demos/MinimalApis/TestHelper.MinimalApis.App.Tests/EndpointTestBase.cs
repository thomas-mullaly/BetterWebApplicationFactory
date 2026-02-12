using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TestHelper.MinimalApis.App.Data;
using TestHelper.MinimalApis.App.Identity;

namespace TestHelper.MinimalApis.App.Tests;

[CollectionDefinition(nameof(EndpointTestCollectionDefinition))]
public class EndpointTestCollectionDefinition : ICollectionFixture<EndpointTestFixture>;

[Collection(nameof(EndpointTestCollectionDefinition))]
public abstract class EndpointTestBase : AspnetTestBase<MinimalApiClientFactory, MinimalApiWebApplicationFactory, Program>, IAsyncLifetime
{
    protected AppDbContext SetupDbContext { get; private set; } = default!;
    protected AppDbContext AssertionDbContext { get; private set; } = default!;
    protected AppDbContext TestDbContext { get; private set; } = default!;
    protected IdentityContext SetupIdentityContext { get; private set; } = default!;
    protected IdentityContext AssertionIdentityContext { get; private set; } = default!;
    protected IdentityContext TestIdentityContext { get; private set; } = default!;
    protected TestAuthHelper AuthHelper { get; private set; } = default!;
    private AsyncServiceScope _scope;

    protected override MinimalApiClientFactory ClientFactory
    {
        get
        {
            return new MinimalApiClientFactory(TestId, WebAppFactory, AuthHelper)
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

        _scope = WebAppFactory.Services.CreateAsyncScope();
        AuthHelper = new TestAuthHelper(_scope.ServiceProvider, SetupIdentityContext, SetupDbContext);

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
        await _scope.DisposeAsync();
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