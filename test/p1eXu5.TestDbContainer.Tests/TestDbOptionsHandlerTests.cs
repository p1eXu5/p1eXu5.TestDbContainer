using Docker.DotNet.Models;
using p1eXu5.CliBootstrap.CommandLineParser;
using p1eXu5.TestDbContainer.Interfaces;
using TestDbContainer;

namespace p1eXu5.TestDbContainer.Tests;

public class TestDbOptionsHandlerTests
{
    private TestDbOptionsHandler _sut = default!;

    private IPhysicalDirectory _physicalDirectory;
    private IDockerContainer _dockerContainer;
    private IDbContext _dbContext;
    private IDotnetCli _dotnetCli;

    private SuccessParsingResult.Success<TestDbOptions> _successParsingResult = default!;

    private SetupMocks Setup { get; set; }

    private AssertMocks Assert { get; set; }

    [SetUp]
    public void Initialize()
    {
        _physicalDirectory = Substitute.For<IPhysicalDirectory>();
        _physicalDirectory.Exists(default!).ReturnsForAnyArgs(true);
        _physicalDirectory.LastWriteTimeUtc(default!).ReturnsForAnyArgs(DateTime.Now);

        _dockerContainer = Substitute.For<IDockerContainer>();
        _dbContext = Substitute.For<IDbContext>();
        _dbContext.GetDateModifiedAsync(default!).ReturnsForAnyArgs(DateTime.Now);

        _dotnetCli = Substitute.For<IDotnetCli>();
        _dotnetCli.UpdateDatabase(default!).ReturnsForAnyArgs(0);

        // _dockerContainer = Substitute.For<IDockerContainer>();
        _sut = new TestDbOptionsHandler(_physicalDirectory, _dockerContainer, _dbContext, _dotnetCli);
        _successParsingResult = new SuccessParsingResult.Success<TestDbOptions>(TestDbOptions.CoreDomainTestDb);

        Setup = new SetupMocks(this);
        Assert = new AssertMocks(this);
    }

    [Test]
    public async Task MigrationFolderDoesNotExist_Throws()
    {
        // Arrange:
        Setup.MigrationFolderDoesNotExist();

        // Action:
        var action = () => _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await action.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    // -------------------- container does not exist

    [Test]
    public async Task ContainerDoesNotExist_CreatesContainer()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await _dockerContainer.ReceivedWithAnyArgs().CreateContainerAsync(default!);
    }

    [Test]
    public async Task ContainerDoesNotExist_StartsContainer()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.CreatedContainerHasBeenStarted();
    }

    [Test]
    public async Task ContainerDoesNotExist_UpdatesDatabase()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        _dotnetCli.ReceivedWithAnyArgs().UpdateDatabase(default!);
    }

    [Test]
    public async Task ContainerDoesNotExist_PhysicalTimeIsLessThenDbTime_UpdatesDbLastWriteTimeUtc()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        DateTime physicalLastWriteTimeUtc = DateTime.Now;
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.DbLastWriteTimeUtcHasBeenUpdated(physicalLastWriteTimeUtc);
    }

    [Test]
    public async Task ContainerDoesNotExist_PhysicalTimeIsGreaterThenDbTime_UpdatesDbLastWriteTimeUtc()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        DateTime physicalLastWriteTimeUtc = DateTime.Now;
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.DbLastWriteTimeUtcHasBeenUpdated(physicalLastWriteTimeUtc);
    }

    [Test]
    public async Task ContainerDoesNotExist_NoDbRecords_CreatesDbLastWriteTimeUtc()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        DateTime physicalLastWriteTimeUtc = DateTime.Now;
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(null);

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await _dbContext.Received()
            .CreateLastWriteTimeUtcAsync(_successParsingResult.Options.ContainerName, physicalLastWriteTimeUtc);
    }

    // -------------------- container exists

    [Test]
    public async Task ContainerExistsAndStopped_LastWriteTimesAreEqual_StartsContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc);

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.StoppedContainerHasBeenStarted();
    }

    // --------------------- stopped container, physical time is less

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsLessThenDbTime_UpdatesDbLastWriteTimeUtc()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.DbLastWriteTimeUtcHasBeenUpdated(physicalLastWriteTimeUtc);
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsLessThenDbTime_RemovesContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.StoppedContainerHasBeenRemoved();
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsLessThenDbTime_CreatesContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.ContainerHasBeenCreated();
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsLessThenDbTime_StartsContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.CreatedContainerHasBeenStarted();
    }

    // --------------------- stopped container, physical time is greater

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsGreaterThenDbTime_UpdatesDbLastWriteTimeUtc()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.DbLastWriteTimeUtcHasBeenUpdated(physicalLastWriteTimeUtc);
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsGreaterThenDbTime_RemovesContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.StoppedContainerHasBeenRemoved();
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsGreaterThenDbTime_CreatesContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.ContainerHasBeenCreated();
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsGreaterThenDbTime_StartsContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.PhysicalLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.HandleAsync(_successParsingResult, CancellationToken.None);

        // Assert:
        await Assert.CreatedContainerHasBeenStarted();
    }


    // --------------------- mock helper classes

    private sealed class SetupMocks
    {
        private readonly TestDbOptionsHandlerTests _;

        internal SetupMocks(TestDbOptionsHandlerTests testDbOptionsHandlerTests)
        {
            _ = testDbOptionsHandlerTests;
        }

        internal void ContainerDoesNotExist()
        {
            _._dockerContainer.FindAsync(default!).ReturnsForAnyArgs((ContainerListResponse?)null);
        }

        internal void ContainerExists()
        {
            _._dockerContainer.FindAsync(default!).ReturnsForAnyArgs(new ContainerListResponse());
        }

        internal void ContainerIsStopped()
        {
            _._dockerContainer.IsRunning(default!).ReturnsForAnyArgs(false);
        }

        internal void MigrationFolderDoesNotExist()
        {
            _._physicalDirectory.Exists(_._successParsingResult.Options.MigrationPath).Returns(false);
        }

        internal void PhysicalLastWriteTimeUtc(DateTime physicalLastWriteTimeUtc)
        {
            _._physicalDirectory.LastWriteTimeUtc(default!).ReturnsForAnyArgs(physicalLastWriteTimeUtc);
        }

        internal void DbLastWriteTimeUtc(DateTime? physicalLastWriteTimeUtc)
        {
            _._dbContext.GetDateModifiedAsync(default!).ReturnsForAnyArgs(physicalLastWriteTimeUtc);
        }
    }

    private sealed class AssertMocks
    {
        private readonly TestDbOptionsHandlerTests _;

        internal AssertMocks(TestDbOptionsHandlerTests testDbOptionsHandlerTests)
        {
            _ = testDbOptionsHandlerTests;
        }

        internal async Task ContainerHasBeenCreated()
        {
            await _._dockerContainer.ReceivedWithAnyArgs().CreateContainerAsync(default!);
        }

        internal async Task CreatedContainerHasBeenStarted()
        {
            await _._dockerContainer.ReceivedWithAnyArgs().StartContainerAsync(default(CreateContainerResponse)!);
        }

        internal async Task StoppedContainerHasBeenStarted()
        {
            await _._dockerContainer.ReceivedWithAnyArgs().StartContainerAsync(default(ContainerListResponse)!);
        }

        internal async Task StoppedContainerHasBeenRemoved()
        {
            await _._dockerContainer.ReceivedWithAnyArgs().RemoveContainerAsync(default(ContainerListResponse)!, default);
        }
     
        internal async Task DbLastWriteTimeUtcHasBeenUpdated(DateTime dateTime)
        {
            await _._dbContext.Received()
                .UpdateLastWriteTimeUtcAsync(_._successParsingResult.Options.ContainerName, dateTime);
        }
    }
}