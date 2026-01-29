namespace TestHelper.MinimalApis.App.Tests;

public class EndpointTestFixture
{
    public MinimalApiWebApplicationFactory Factory { get; }
    public TestContainerFixture TestContainerFixture { get; }

    public EndpointTestFixture(TestContainerFixture testContainerFixture)
    {
        Factory = new();
        TestContainerFixture = testContainerFixture;
    }
}