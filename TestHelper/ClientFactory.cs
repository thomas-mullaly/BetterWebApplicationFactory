using Microsoft.Extensions.DependencyInjection;

namespace TestHelper;

public abstract class ClientFactoryBase;

public class ClientFactory<TProgram, TClientFactoryImpl> : ClientFactoryBase
    where TProgram : class
    where TClientFactoryImpl : ClientFactory<TProgram, TClientFactoryImpl>
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

    public TClientFactoryImpl AddServiceOverride(Action<IServiceCollection> serviceOverride)
    {
        _serviceOverrides.Add(serviceOverride);
        return (TClientFactoryImpl)this;
    }

    public HttpClient CreateClient()
    {
        var client = _webApplicationFactory.OverrideRequestServices(_testId, services =>
            {
                _serviceOverrides.ForEach(serviceOverride => serviceOverride(services));
            })
            .CreateClient();
        client.DefaultRequestHeaders.Add(_webApplicationFactory.RequestIdHeader, _testId.ToString());

        CustomizeHttpClient(client);

        return client;
    }

    protected virtual void CustomizeHttpClient(HttpClient client) { }
}