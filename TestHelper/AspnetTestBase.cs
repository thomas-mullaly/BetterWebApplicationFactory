namespace TestHelper;

public abstract class AspnetTestBase<TClientFactory, TWebAppFactory, TProgram>
    where TClientFactory : ClientFactory<TProgram, TClientFactory>
    where TWebAppFactory : BetterWebAppFactory<TProgram>
    where TProgram : class
{
    protected TWebAppFactory WebAppFactory { get; }
    protected abstract TClientFactory ClientFactory { get; }
    protected Guid TestId => Guid.NewGuid();

    protected AspnetTestBase(TWebAppFactory factory)
    {
        WebAppFactory = factory;
    }
}