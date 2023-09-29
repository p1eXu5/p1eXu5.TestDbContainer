using p1eXu5.CliBootstrap;
using p1eXu5.CliBootstrap.CommandLineParser;
using p1eXu5.TestDbContainer.Options;

namespace p1eXu5.TestDbContainer.Handlers;

internal class OptionsHandler : IOptionsHandler
{
    private readonly ContainerVerbHandler _containerVerbHandler;
    private readonly ComposeVerbHandler _composeVerbHandler;

    public OptionsHandler(
        ContainerVerbHandler containerVerbHandler,
        ComposeVerbHandler composeVerbHandler)
    {
        _containerVerbHandler = containerVerbHandler;
        _composeVerbHandler = composeVerbHandler;
    }

    public Task HandleAsync(SuccessParsingResult successParsingResult, CancellationToken cancellationToken) => successParsingResult switch
    {
        SuccessParsingResult.Success<ContainerVerb> success => _containerVerbHandler.ProcessAsync(success.Options, cancellationToken),
        SuccessParsingResult.Success<ComposeVerb> success => _composeVerbHandler.ProcessAsync(success.Options, cancellationToken),
        _ => Task.CompletedTask,
    };
}
