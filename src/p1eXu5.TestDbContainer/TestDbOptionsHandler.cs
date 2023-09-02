using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using p1eXu5.CliBootstrap;
using p1eXu5.CliBootstrap.CommandLineParser;
using p1eXu5.TestDbContainer.Interfaces;

namespace TestDbContainer;

internal class TestDbOptionsHandler : IOptionsHandler
{
    private readonly IPhysicalDirectory _physicalDirectory;
    private readonly IDockerContainer _dockerContainer;
    private readonly IDbContext _dbContext;
    private readonly IDotnetCli _dotnetCli;
    private readonly ILogger _logger;

    public TestDbOptionsHandler(
        IPhysicalDirectory physicalDirectory,
        IDockerContainer dockerContainer,
        IDbContext dbContext,
        IDotnetCli dotnetCli,
        ILogger logger)
    {
        _physicalDirectory = physicalDirectory;
        _dockerContainer = dockerContainer;
        _dbContext = dbContext;
        _dotnetCli = dotnetCli;
        _logger = logger;
    }

    public Task HandleAsync(SuccessParsingResult successParsingResult, CancellationToken cancellationToken) => successParsingResult switch
    {
        SuccessParsingResult.Success<TestDbOptions> success => ProcessAsync(success.Options, cancellationToken),
        _ => Task.CompletedTask,
    };

    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    public async Task ProcessAsync(TestDbOptions testDb, CancellationToken cancellationToken)
    {
        if (!_physicalDirectory.Exists(testDb.MigrationPath))
        {
            string error = $"Directory does not exists!: {testDb.MigrationPath}";
            _logger.LogError("Error: {Error}", error);
            throw new DirectoryNotFoundException(error);
        }

        var modifiedDate = _physicalDirectory.LastWriteTimeUtc(testDb.MigrationPath);
        var dbModifiedDate = await _dbContext.GetDateModifiedAsync(testDb.ContainerName).ConfigureAwait(false);

        Docker.DotNet.Models.ContainerListResponse? container;

        try
        {
            container = await _dockerContainer.FindAsync(testDb.ContainerName, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            _logger.LogError("Docker access error! Check that docker daemon is running.");
            return;
        }

        if (container is null)
        {
            await CreateOrUpdateDbLastWriteTimeUtcAsync().ConfigureAwait(false);
            await CreateAndStartContainer().ConfigureAwait(false);

            return;
        }

        if (await CreateOrUpdateDbLastWriteTimeUtcAsync().ConfigureAwait(false))
        {
            _logger.LogInformation("Removing container...");
            await _dockerContainer.RemoveContainerAsync(container, cancellationToken).ConfigureAwait(false);
            await CreateAndStartContainer().ConfigureAwait(false);

            return;
        }

        if (!_dockerContainer.IsRunning(container))
        {
            _logger.LogInformation("Running container...");
            await _dockerContainer.StartContainerAsync(container, cancellationToken).ConfigureAwait(false);
        }
        _logger.LogInformation("Ready.");

        // --------------------- locals

        async Task<bool> CreateOrUpdateDbLastWriteTimeUtcAsync()
        {
            if (dbModifiedDate.NotHasValue())
            {
                await _dbContext.CreateLastWriteTimeUtcAsync(testDb.ContainerName, modifiedDate).ConfigureAwait(false);
                return true;
            }
            else if (DatesAreNotEqual())
            {
                await _dbContext.UpdateLastWriteTimeUtcAsync(testDb.ContainerName, modifiedDate).ConfigureAwait(false);
                return true;
            }

            return false;

        }

        bool DatesAreEqual()
            => (modifiedDate - dbModifiedDate!.Value) < TimeSpan.FromMilliseconds(1);

        bool DatesAreNotEqual()
            => !DatesAreEqual();

        async Task CreateAndStartContainer()
        {
            _logger.LogInformation("Creating container...");
            var createContainerResponse = await _dockerContainer.CreateContainerAsync(testDb, cancellationToken).ConfigureAwait(false);
            await _dockerContainer.StartContainerAsync(createContainerResponse, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Applying migrations...");
            _dotnetCli.UpdateDatabase(testDb);
        }
    }
}
