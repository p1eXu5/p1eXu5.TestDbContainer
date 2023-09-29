using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using p1eXu5.TestDbContainer.Exceptions;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Models;
using p1eXu5.TestDbContainer.Options;
using TestDbContainer;

namespace p1eXu5.TestDbContainer.Handlers;

internal class ComposeVerbHandler : VerbHandlerBase<ComposeVerb>
{
    private readonly IComposeFileParser _composeFileParser;
    private ComposeFile? _composeFile;
    private readonly object _lock = new();

    public ComposeVerbHandler(
        IMigrationFolder migrationFolder,
        IDbContext dbContext,
        IDockerContainer dockerContainer,
        IComposeFileParser composeFileParser,
        IDotnetCli dotnetCli,
        ILogger logger)
        : base(migrationFolder, dbContext, dotnetCli, dockerContainer, logger)
    {
        _composeFileParser = composeFileParser;
    }

    protected override async Task<ContainerListResponse?> TryFindExistingContainer(ComposeVerb composeVerb, CancellationToken cancellationToken)
    {
        ComposeFile composeFile = GetComposeFile(composeVerb);
        string containerName = $"{composeFile.Name}-{composeVerb.ContainerName}";
        try
        {
            return await DockerContainer.FindAsync(containerName, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var errorMessage = "Searching container in docker is failed! Check that docker daemon is running.";
            Logger.LogError(ex, "Error: {Error}", errorMessage);
            throw new DockerOperationFailedException(errorMessage, ex);
        }
    }

    protected override void CheckVerb(ComposeVerb composeVerb)
    {
        base.CheckVerb(composeVerb);
        CheckComposeFileExists(composeVerb.ComposeFilePath);

        ComposeFile composeFile = GetComposeFile(composeVerb);

        if (!composeFile.ServiceExists(composeVerb.ContainerName))
        {
            Logger.LogError("Service {Servcie} is absent in compose file {ComposeFile}", composeVerb.ContainerName, composeVerb.ComposeFile);
            throw new ServiceNotFoundException($"Service {composeVerb.ContainerName} is absent in compose file {composeVerb.ComposeFile}");
        }
    }

    internal override string GetMigrationAlias(ComposeVerb composeVerb)
        => $"{Path.GetFileNameWithoutExtension(composeVerb.ComposeFile)}-{composeVerb.ContainerName}";

    protected override Task CreateAndStartContainer(ComposeVerb composeVerb, CancellationToken cancellationToken)
    {
        DotnetCli.Compose(composeVerb);
        return Task.CompletedTask;
    }

    private ComposeFile GetComposeFile(ComposeVerb composeVerb)
    {
        lock (_lock)
        {
            _composeFile ??= _composeFileParser.Parse(composeVerb.ComposeFilePath);
            return _composeFile;
        }
    }

    private void CheckComposeFileExists(ComposeFilePath composeFilePath)
    {
        if (!_composeFileParser.FileExists(composeFilePath))
        {
            string error = $"Compose fil does not exists!: {composeFilePath.Path}";
            Logger.LogError("Error: {Error}", error);
            throw new ComposeFileNotFoundException(error);
        }
    }

    protected override void UpdateDatabase(ComposeVerb composeVerb)
    {
        ComposeFile composeFile = GetComposeFile(composeVerb);
        DotnetCli.UpdateDatabase(composeVerb, localIp => composeFile.MySqlConnectionString(localIp, composeVerb.ContainerName));
    }
}
