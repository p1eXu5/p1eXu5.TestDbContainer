using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        services.TryAddSingleton<IOptionsHandler>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("test-db");
            return
                (IOptionsHandler)ActivatorUtilities.CreateInstance(sp, typeof(TestDbOptionsHandler), logger);
        });
        services.TryAddSingleton<IDockerContainer, DockerContainer>();
        services.TryAddSingleton<IPhysicalDirectory, PhysicalDirectory>();
        services.TryAddSingleton<IDbContext>(DbContext.Instance);
        services.TryAddSingleton<IDotnetCli, DotnetCli>();
    }

    protected override void ConfigureConsoleLogging(HostBuilderContext context, ILoggingBuilder loggingBuilder, ParsingResult parsingResult)
    {
        base.ConfigureConsoleLogging(context, loggingBuilder, parsingResult);

        var options = parsingResult switch
        {
            ParsingResult.Success<TestDbOptions> s => s.Options,
            _ => null
        };

        if (context.HostingEnvironment.IsProduction() && (options is null || !options.Verbose))
        {
            loggingBuilder.AddFilter((context, level) =>
            {
                if (context?.StartsWith("test-db", StringComparison.Ordinal) == true)
                {
                    return true;
                }

                return false;
            });
        }
    }
}
