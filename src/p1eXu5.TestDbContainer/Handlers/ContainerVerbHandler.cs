using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using p1eXu5.TestDbContainer.Exceptions;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Options;

namespace p1eXu5.TestDbContainer.Handlers;

internal class ContainerVerbHandler : VerbHandlerBase<ContainerVerb>
{
    private readonly IDotnetCli _dotnetCli;

    public ContainerVerbHandler(
        IMigrationFolder migrationFolder,
        IDbContext dbContext,
        IDockerContainer dockerContainer,
        IDotnetCli dotnetCli,
        ILogger logger)
        : base(migrationFolder, dbContext, dotnetCli, dockerContainer, logger)
    {
        _dotnetCli = dotnetCli;
    }

    protected override async Task<ContainerListResponse?> TryFindExistingContainer(ContainerVerb containerVerb, CancellationToken cancellationToken)
    {
        try
        {
            return await DockerContainer.FindAsync(containerVerb.ContainerName, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var errorMessage = "Searching container in docker is failed! Check that docker daemon is running.";
            Logger.LogError(ex, "Error: {Error}", errorMessage);
            throw new DockerOperationFailedException(errorMessage, ex);
        }
    }

    internal override string GetMigrationAlias(ContainerVerb containerVerb)
        => containerVerb.ContainerName;

    protected override async Task CreateAndStartContainer(ContainerVerb containerVerb, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Creating container...");
        var createContainerResponse = await DockerContainer.CreateContainerAsync(containerVerb, cancellationToken).ConfigureAwait(false);

        Logger.LogInformation("Starting container...");
        await DockerContainer.StartContainerAsync(createContainerResponse, cancellationToken).ConfigureAwait(false);
    }

    protected override void UpdateDatabase(ContainerVerb verb)
    {
        Logger.LogInformation("Applying migrations...");
        _dotnetCli.UpdateDatabase(verb, verb.MySqlConnectionString);
    }
}
