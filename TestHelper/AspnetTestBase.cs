namespace TestHelper;

public abstract class AspnetTestBase<TClientFactory, TProgram>
    where TClientFactory : ClientFactoryBase
    where TProgram : class
{
    protected BetterWebAppFactory<TProgram> WebAppFactory { get; }
    protected abstract TClientFactory ClientFactory { get; }
    protected Guid TestId => Guid.NewGuid();

    protected AspnetTestBase(BetterWebAppFactory<TProgram> factory)
    {
        WebAppFactory = factory;
    }
}