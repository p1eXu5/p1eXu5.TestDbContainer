using Docker.DotNet.Models;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Models;
using p1eXu5.TestDbContainer.Options;

namespace p1eXu5.TestDbContainer.Tests.Unit.Handlers;

public abstract class VerbHandlerTestsBase
{
    private protected IMigrationFolder _migrationFolder = default!;
    private protected IDotnetCli _dotnetCli = default!;
    private protected IDockerContainer _dockerContainer = default!;
    private protected IDbContext _dbContext = default!;

    // --------------------- mock helper classes

    protected class SetupMocksBase<TTests> where TTests : VerbHandlerTestsBase
    {
        private protected readonly TTests _;

        internal SetupMocksBase(TTests testDbOptionsHandlerTests)
        {
            _ = testDbOptionsHandlerTests;

            _._migrationFolder = Substitute.For<IMigrationFolder>();
            _._migrationFolder.Exists(default!).ReturnsForAnyArgs(true);
            _._migrationFolder.IsEmpty(default!).ReturnsForAnyArgs(false);
            _._migrationFolder.LastWriteTimeUtc(default!).ReturnsForAnyArgs(DateTime.Now);

            _._dotnetCli = Substitute.For<IDotnetCli>();
            _._dotnetCli.CreateInitialMigration(default!).ReturnsForAnyArgs(0);
            _._dotnetCli.UpdateDatabase(default!, default!).ReturnsForAnyArgs(0);

            _._dockerContainer = Substitute.For<IDockerContainer>();

            _._dbContext = Substitute.For<IDbContext>();
            _._dbContext.GetDateModifiedAsync(default!).ReturnsForAnyArgs(DateTime.Now);
        }

        internal void ContainerDoesNotExist()
        {
            _._dockerContainer.FindAsync(default!, default).ReturnsForAnyArgs((ContainerListResponse?)null);
        }

        internal void ContainerExists()
        {
            _._dockerContainer.FindAsync(default!, default).ReturnsForAnyArgs(new ContainerListResponse());
        }

        internal void ContainerIsStopped()
        {
            _._dockerContainer.IsRunning(default!).ReturnsForAnyArgs(false);
        }

        internal void MigrationFolderDoesNotExist()
        {
            _._migrationFolder.Exists(default!).ReturnsForAnyArgs(false);
        }

        internal void MigrationFolderIsEmpty()
        {
            _._migrationFolder.IsEmpty(default!).ReturnsForAnyArgs(true);
        }

        internal void MigrationFolderLastWriteTimeUtc(DateTime migrationFolderLastWriteTimeUtc)
        {
            _._migrationFolder.LastWriteTimeUtc(default!).ReturnsForAnyArgs(migrationFolderLastWriteTimeUtc);
        }

        internal void DbLastWriteTimeUtc(DateTime? dbLastWriteTimeUtc)
        {
            _._dbContext.GetDateModifiedAsync(default!).ReturnsForAnyArgs(dbLastWriteTimeUtc);
        }
    }

    protected class AssertMocksBase<TTests> where TTests : VerbHandlerTestsBase
    {
        private protected readonly TTests _;

        internal AssertMocksBase(TTests testDbOptionsHandlerTests)
        {
            _ = testDbOptionsHandlerTests;
        }

        internal async Task ContainerHasBeenCreated()
        {
            await _._dockerContainer.ReceivedWithAnyArgs()
                .CreateContainerAsync(default!, default);
        }

        internal async Task CreatedContainerHasBeenStarted()
        {
            await _._dockerContainer.ReceivedWithAnyArgs()
                .StartContainerAsync(default(CreateContainerResponse)!, default);
        }

        internal async Task ContainerHasBeenStarted()
        {
            await _._dockerContainer.ReceivedWithAnyArgs()
                .StartContainerAsync(default(ContainerListResponse)!, default);
        }

        internal async Task ContainerHasBeenRemoved()
        {
            await _._dockerContainer.ReceivedWithAnyArgs()
                .RemoveContainerAsync(default(ContainerListResponse)!, default);
        }

        internal async Task DbLastWriteTimeUtcHasBeenUpdated(DateTime dateTime)
        {
            await _._dbContext.Received()
                .UpdateLastWriteTimeUtcAsync(Arg.Any<string>(), dateTime);
        }

        internal void InitMigrationHasBeenCreated()
        {
            _._dotnetCli.ReceivedWithAnyArgs(1).CreateInitialMigration(default!);
        }

        internal void ContainerHasBeenComposed(ComposeVerb composeVerb)
            => _._dotnetCli.Received(1).Compose(composeVerb);

        internal void DatabaseHasBeenUpdated(TestDbOptionsBase testDbOptionsBase)
            => _._dotnetCli.Received(1).UpdateDatabase(testDbOptionsBase, Arg.Any<Func<LocalIP, string>>());
    }
}
