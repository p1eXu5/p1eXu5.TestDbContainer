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

    public TestDbOptionsHandler(
        IPhysicalDirectory physicalDirectory,
        IDockerContainer dockerContainer,
        IDbContext dbContext,
        IDotnetCli dotnetCli)
    {
        _physicalDirectory = physicalDirectory;
        _dockerContainer = dockerContainer;
        _dbContext = dbContext;
        _dotnetCli = dotnetCli;
    }

    public Task HandleAsync(SuccessParsingResult successParsingResult, CancellationToken cancellationToken) => successParsingResult switch
    {
        SuccessParsingResult.Success<TestDbOptions> success => ProcessAsync(success.Options, cancellationToken),
        _ => Task.CompletedTask,
    };

    public async Task ProcessAsync(TestDbOptions testDb, CancellationToken cancellationToken)
    {
        if (!_physicalDirectory.Exists(testDb.MigrationPath))
        {
            string error = $"Directory does not exists!: {testDb.MigrationPath}";
            Console.WriteLine(error);
            throw new DirectoryNotFoundException(error);
        }

        var modifiedDate = _physicalDirectory.LastWriteTimeUtc(testDb.MigrationPath);
        var dbModifiedDate = await _dbContext.GetDateModifiedAsync(testDb.ContainerName);
        var container = await _dockerContainer.FindAsync(testDb.ContainerName);

        if (container is null)
        {
            await CreateOrUpdateDbLastWriteTimeUtcAsync();
            await CreateAndStartContainer();

            return;
        }

        if (await CreateOrUpdateDbLastWriteTimeUtcAsync())
        {
            await _dockerContainer.RemoveContainerAsync(container, cancellationToken);
            await CreateAndStartContainer();

            return;
        }

        if (!_dockerContainer.IsRunning(container))
        {
            await _dockerContainer.StartContainerAsync(container);
        }

        // --------------------- locals

        async Task<bool> CreateOrUpdateDbLastWriteTimeUtcAsync()
        {
            if (dbModifiedDate.NotHasValue())
            {
                await _dbContext.CreateLastWriteTimeUtcAsync(testDb.ContainerName, modifiedDate);
                return true;
            }
            else if (dbModifiedDate!.Value != modifiedDate)
            {
                await _dbContext.UpdateLastWriteTimeUtcAsync(testDb.ContainerName, modifiedDate);
                return true;
            }

            return false;
        }

        async Task CreateAndStartContainer()
        {
            var createContainerResponse = await _dockerContainer.CreateContainerAsync(testDb);
            await _dockerContainer.StartContainerAsync(createContainerResponse);
            _dotnetCli.UpdateDatabase(testDb);
        }
    }
}
