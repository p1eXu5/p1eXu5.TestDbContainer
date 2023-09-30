using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using p1eXu5.CliBootstrap;
using p1eXu5.CliBootstrap.CommandLineParser;
using p1eXu5.Result.Extensions;
using p1eXu5.TestDbContainer.Handlers;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Options;
using TestDbContainer;

namespace p1eXu5.TestDbContainer;

internal class TestDbContainerBootstrap : Bootstrap
{
    protected virtual VerbBuilder VerbBuilder => new VerbBuilder();

    protected override ParsingResult ParseCommandLineArguments(string[] args)
    {
        var parsingResult = ArgsParser.Parse<ContainerVerb, ComposeVerb>(args);

        if (parsingResult is ParsingResult.Success<ContainerVerb> containerVerb)
        {
            return Confirm(containerVerb);
        }

        if (parsingResult is ParsingResult.Success<ComposeVerb> composeVerb)
        {
            return Confirm(composeVerb);
        }

        return parsingResult;
    }

    private ParsingResult Confirm<TVerb>(ParsingResult.Success<TVerb> success)
        where TVerb : TestDbOptionsBase
    {
        var confirmResult = VerbBuilder.BuildVerb(success.Options);
        if (confirmResult.TryGetSuccessContext(out var confirmContext))
        {
            return new ParsingResult.Success<TVerb>(confirmContext);
        }
        else
        {
            return new ParsingResult.Error(confirmResult.FailedContext());
        }
    }

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ParsingResult parsingResult)
    {
        base.ConfigureServices(services, configuration, parsingResult);
        services.TryAddSingleton<IOptionsHandler, OptionsHandler>();
        services.TryAddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("test-db");
            return
                (ContainerVerbHandler)ActivatorUtilities.CreateInstance(sp, typeof(ContainerVerbHandler), logger);
        });
        services.TryAddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("test-db");
            return
                (ComposeVerbHandler)ActivatorUtilities.CreateInstance(sp, typeof(ComposeVerbHandler), logger);
        });
        services.TryAddSingleton<IDockerContainer, DockerContainer>();
        services.TryAddSingleton<IMigrationFolder, MigrationFolder>();
        services.TryAddSingleton<IDbContext>(DbContext.Instance);
        services.TryAddSingleton<IDotnetCli, DotnetCli>();
        services.TryAddSingleton<IComposeFileParser, ComposeFileParser>();
    }

    protected override void ConfigureConsoleLogging(HostBuilderContext context, ILoggingBuilder loggingBuilder, ParsingResult parsingResult)
    {
        base.ConfigureConsoleLogging(context, loggingBuilder, parsingResult);

        var options = parsingResult switch
        {
            ParsingResult.Success<TestDbOptionsBase> s => s.Options,
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

    protected override void OnApplicationStarting(IHost host, ParsingResult parsingResult)
    {
        var logger = GetAppLogger(host.Services);
        logger.LogInformation("test-db container started...");
    }
}
