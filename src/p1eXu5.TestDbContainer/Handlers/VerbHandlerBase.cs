using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using p1eXu5.TestDbContainer.Exceptions;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Options;
using TestDbContainer;

namespace p1eXu5.TestDbContainer.Handlers;

internal abstract class VerbHandlerBase<TVerb> where TVerb : TestDbOptionsBase
{
    private readonly IMigrationFolder _migrationFolder;
    private readonly IDbContext _dbContext;

    public VerbHandlerBase(IMigrationFolder migrationFolder, IDbContext dbContext, IDotnetCli dotnetCli, IDockerContainer dockerContainer, ILogger logger)
    {
        _migrationFolder = migrationFolder;
        _dbContext = dbContext;
        DotnetCli = dotnetCli;
        DockerContainer = dockerContainer;
        Logger = logger;
    }

    protected IDotnetCli DotnetCli { get; }

    protected IDockerContainer DockerContainer { get; }

    protected ILogger Logger { get; }

    // ------------------

    public virtual async Task ProcessAsync(TVerb verb, CancellationToken cancellationToken)
    {
        CheckVerb(verb);

        if (_migrationFolder.IsEmpty(verb.MigrationFolderPath))
        {
            DotnetCli.CreateInitialMigration(verb);
        }

        string migrationAlias = GetMigrationAlias(verb);
        bool migrationsFolderHasBeenUpdated = await CreateOrUpdateDbLastWriteTimeUtcAsync(migrationAlias, verb).ConfigureAwait(false);

        ContainerListResponse? container = await TryFindExistingContainer(verb, cancellationToken).ConfigureAwait(false);

        if (container is null)
        {
            await CreateAndStartContainer(verb, cancellationToken).ConfigureAwait(false);
            UpdateDatabase(verb);
            return;
        }

        if (migrationsFolderHasBeenUpdated)
        {
            Logger.LogInformation("Removing container...");
            await DockerContainer.RemoveContainerAsync(container, cancellationToken).ConfigureAwait(false);

            await CreateAndStartContainer(verb, cancellationToken).ConfigureAwait(false);
            UpdateDatabase(verb);
            return;
        }

        if (!DockerContainer.IsRunning(container))
        {
            Logger.LogInformation("Running container...");
            await DockerContainer.StartContainerAsync(container, cancellationToken).ConfigureAwait(false);
        }

        Logger.LogInformation("Ready.");
    }

    // ------------------

    protected void CheckMigrationFolderExists(TestDbOptionsBase testDb)
    {
        if (!_migrationFolder.Exists(testDb.MigrationFolderPath))
        {
            string error = $"Migration folder does not exists!: {testDb.MigrationFolderPath}";
            Logger.LogError("Error: {Error}", error);
            throw new MigrationFolderNotFoundException(error);
        }
    }

    protected virtual void CheckVerb(TVerb verb)
    {
        CheckMigrationFolderExists(verb);
    }

    internal abstract string GetMigrationAlias(TVerb verb);

    protected abstract void UpdateDatabase(TVerb verb);

    protected abstract Task<ContainerListResponse?> TryFindExistingContainer(TVerb verb, CancellationToken cancellationToken);

    protected abstract Task CreateAndStartContainer(TVerb verb, CancellationToken cancellationToken);


    protected static bool DatesAreEqual(DateTime currentDate, DateTime lastDate)
        => Math.Abs((currentDate - lastDate).TotalMilliseconds) < 1;

    protected static bool DatesAreNotEqual(DateTime currentDate, DateTime lastDate)
           => !DatesAreEqual(currentDate, lastDate);

    protected async Task<bool> CreateOrUpdateDbLastWriteTimeUtcAsync(string migrationAlias, TestDbOptionsBase containerVerb)
    {
        var modifiedDate = _migrationFolder.LastWriteTimeUtc(containerVerb.MigrationFolderPath);
        var dbModifiedDate = await _dbContext.GetDateModifiedAsync(migrationAlias).ConfigureAwait(false);

        if (dbModifiedDate.HasNoValue())
        {
            await _dbContext.CreateLastWriteTimeUtcAsync(migrationAlias, modifiedDate).ConfigureAwait(false);
            return true;
        }
        else if (DatesAreNotEqual(modifiedDate, dbModifiedDate.Value))
        {
            await _dbContext.UpdateLastWriteTimeUtcAsync(migrationAlias, modifiedDate).ConfigureAwait(false);
            return true;
        }

        return false;
    }
}
