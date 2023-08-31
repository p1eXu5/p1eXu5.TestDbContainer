using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using p1eXu5.CliBootstrap;
using p1eXu5.CliBootstrap.CommandLineParser;
using p1eXu5.TestDbContainer.Interfaces;
using TestDbContainer;

namespace p1eXu5.TestDbContainer;

internal sealed class TestDbContainerBootstrap : Bootstrap
{
    protected override ParsingResult ParseCommandLineArguments(string[] args)
    {
        return ArgsParser.Parse<TestDbOptions>(args);
    }

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ParsingResult parsingResult)
    {
        base.ConfigureServices(services, configuration, parsingResult);

        services.TryAddSingleton<IOptionsHandler, TestDbOptionsHandler>();
        services.TryAddSingleton<IDockerContainer, DockerContainer>();
        services.TryAddSingleton<IPhysicalDirectory, PhysicalDirectory>();
        services.TryAddSingleton<IDbContext>(DbContext.Instance);
        services.TryAddSingleton<IDotnetCli, DotnetCli>();
    }
}
