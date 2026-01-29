using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TestHelper.Filters;

internal sealed class PerRequestDIStartupFilter : IStartupFilter
{
    private readonly IServiceProvider _rootServiceProvider;
    private readonly IServiceCollection _appServices;
    private readonly IDictionary<Guid, IServiceCollection> _testServices;
    private readonly string _requestIdHeader;

    public PerRequestDIStartupFilter(
        IServiceProvider rootServiceProvider,
        IServiceCollection appServices,
        IDictionary<Guid, IServiceCollection> testServices,
        string requestIdHeader)
    {
        _rootServiceProvider = rootServiceProvider;
        _appServices = appServices;
        _testServices = testServices;
        _requestIdHeader = requestIdHeader;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(async (context, middlewareNext) =>
            {
                if (!context.Request.Headers.TryGetValue(_requestIdHeader, out var testId)
                    || string.IsNullOrEmpty(testId)
                    || !Guid.TryParse(testId, out var guidTestId))
                {
                    await middlewareNext(context);
                    return;
                }

                var testServices = _testServices[guidTestId];

                var requestServices = new ServiceCollection();
                foreach (var sd in _appServices)
                {
                    requestServices.Add(sd);
                }

                foreach (var sd in testServices)
                {
                    requestServices.Add(sd);
                }

                var testProvider = requestServices.BuildServiceProvider();
                using var scope = testProvider.CreateScope();

                context.RequestServices = scope.ServiceProvider;
                await middlewareNext(context);
            });

            next(app);
        };
    }
}