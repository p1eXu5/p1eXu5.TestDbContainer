using p1eXu5.TestDbContainer.Interfaces;

namespace p1eXu5.TestDbContainer.Tests.Integration;

public sealed class Sut
{
    private TestDbContainerBootstrapDecorator _bootstrap;

    internal IDbContext MockDbContext => _bootstrap.MockDbContext;
    internal IDockerContainer MockDockerContainer => _bootstrap.MockDockerContainer;
    internal IDotnetCli MockDotnetCli => _bootstrap.MockDotnetCli;
    internal IMigrationFolder MockPhysicalDirectory => _bootstrap.MockPhysicalDirectory;

    internal TestDbContainerBootstrapDecorator Bootstrap
        => _bootstrap ??= new TestDbContainerBootstrapDecorator();

    internal Task RunAsync(params string[] args)
    {
        Bootstrap.TestContextWriters.Out = TestContext.Out;
        Bootstrap.TestContextWriters.Progress = TestContext.Progress;
        return Bootstrap.RunAsync(args);
    }
}
