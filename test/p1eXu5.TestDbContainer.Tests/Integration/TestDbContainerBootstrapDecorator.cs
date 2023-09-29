using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using p1eXu5.AspNetCore.Testing.Logging;
using p1eXu5.CliBootstrap.CommandLineParser;
using p1eXu5.TestDbContainer.Interfaces;
using TestDbContainer;

namespace p1eXu5.TestDbContainer.Tests.Integration;

internal sealed class TestDbContainerBootstrapDecorator : TestDbContainerBootstrap
{
    internal IDbContext MockDbContext { get; } = Substitute.For<IDbContext>();

    internal IDockerContainer MockDockerContainer { get; } = Substitute.For<IDockerContainer>();

    internal IDotnetCli MockDotnetCli { get; } = Substitute.For<IDotnetCli>();

    internal IMigrationFolder MockPhysicalDirectory { get; } = Substitute.For<IMigrationFolder>();

    internal TestContextWriters TestContextWriters { get; } = new TestContextWriters()
    {
        Out = TestContext.Out,
        Progress = TestContext.Progress,
    };

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ParsingResult parsingResult)
    {
        base.ConfigureServices(services, configuration, parsingResult);
        ReplaceWithMock<IDbContext, DbContext>(services, MockDbContext);
        ReplaceWithMock<IDockerContainer, DockerContainer>(services, MockDockerContainer);
        ReplaceWithMock<IDotnetCli, DotnetCli>(services, MockDotnetCli);
        ReplaceWithMock<IMigrationFolder, MigrationFolder>(services, MockPhysicalDirectory);
    }

    protected override void ConfigureConsoleLogging(HostBuilderContext context, ILoggingBuilder loggingBuilder, ParsingResult parsingResult)
    {
        base.ConfigureConsoleLogging(context, loggingBuilder, parsingResult);
        loggingBuilder.AddTestLogger(TestContextWriters, LogOut.Out | LogOut.Progress);
    }

    private void ReplaceWithMock<TServiceType, TImplementation>(IServiceCollection services, TServiceType mock)
        where TServiceType : class
        where TImplementation : TServiceType
    {
        var dbContextDescriptor = services.First(sd => sd.ImplementationType == typeof(TImplementation));
        services.Remove(dbContextDescriptor);
        services.AddSingleton(mock);
    }
}
