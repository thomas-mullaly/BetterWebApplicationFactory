using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TestHelper.Filters;

namespace TestHelper;

public class BetterWebAppFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    private readonly ConcurrentDictionary<Guid, IServiceCollection> _perRequestServiceOverrides = new();

    public virtual string RequestIdHeader => "X-Request-Id";

    public BetterWebAppFactory<TEntryPoint> OverrideRequestServices(Guid testId, Action<IServiceCollection> serviceOverride)
    {
        _perRequestServiceOverrides.TryAdd(testId, new ServiceCollection());
        serviceOverride(_perRequestServiceOverrides[testId]);
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IStartupFilter>(
                sp => new PerRequestDIStartupFilter(sp, services, _perRequestServiceOverrides, RequestIdHeader));
        });

        base.ConfigureWebHost(builder);
    }
}