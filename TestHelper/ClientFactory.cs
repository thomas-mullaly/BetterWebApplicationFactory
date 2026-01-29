using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace TestHelper;

public abstract class ClientFactoryBase;

public class ClientFactory<TProgram> : ClientFactoryBase
    where TProgram : class
{
    private readonly List<Action<IServiceCollection>> _serviceOverrides;
    private readonly Guid _testId;
    private readonly BetterWebAppFactory<TProgram> _webApplicationFactory;

    public ClientFactory(
        Guid testId,
        BetterWebAppFactory<TProgram> webApplicationFactory)
    {
        _testId = testId;
        _webApplicationFactory = webApplicationFactory;
        _serviceOverrides = new();
    }

    public ClientFactory<TProgram> AddServiceOverride(Action<IServiceCollection> serviceOverride)
    {
        _serviceOverrides.Add(serviceOverride);
        return this;
    }

    public HttpClient CreateClient()
    {
        var client = _webApplicationFactory.OverrideRequestServices(_testId, services =>
            {
                _serviceOverrides.ForEach(serviceOverride => serviceOverride(services));
            })
            .CreateClient();
        client.DefaultRequestHeaders.Add(_webApplicationFactory.RequestIdHeader, _testId.ToString());

        return client;
    }
}