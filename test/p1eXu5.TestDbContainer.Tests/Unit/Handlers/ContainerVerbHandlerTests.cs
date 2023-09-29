using AutoBogus;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using p1eXu5.TestDbContainer.Handlers;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Options;

namespace p1eXu5.TestDbContainer.Tests.Unit.Handlers;

public sealed class ContainerVerbHandlerTests : VerbHandlerTestsBase
{
    private ContainerVerbHandler _sut;
    private ContainerVerb _containerVerb;

    private SetupMocks Setup { get; set; }

    private AssertMocks Assert { get; set; }

    [SetUp]
    public void Initialize()
    {
        var logger = Substitute.For<ILogger>();

        Setup = new SetupMocks(this);
        Assert = new AssertMocks(this);

        _sut = new ContainerVerbHandler(_migrationFolder, _dbContext, _dockerContainer, _dotnetCli, logger);

        _containerVerb = AutoFaker.Generate<ContainerVerb>();
    }

    [Test]
    public async Task MigrationFolderDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // Arrange:
        Setup.MigrationFolderDoesNotExist();

        // Action:
        var action = () => _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await action.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Test]
    public async Task MigrationFolderIsEmpty_CallsCreateInitMigration()
    {
        // Arrange:
        Setup.MigrationFolderIsEmpty();

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        Assert.InitMigrationHasBeenCreated();
    }

    // -------------------- container does not exist

    [Test]
    public async Task ContainerDoesNotExist_CreatesContainer()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await _dockerContainer.ReceivedWithAnyArgs().CreateContainerAsync(default!, default);
    }

    [Test]
    public async Task ContainerDoesNotExist_StartsContainer()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await Assert.CreatedContainerHasBeenStarted();
    }

    [Test]
    public async Task ContainerDoesNotExist_UpdatesDatabase()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        _dotnetCli.ReceivedWithAnyArgs().UpdateDatabase(default!, default!);
    }

    [Test]
    public async Task ContainerDoesNotExist_PhysicalTimeIsLessThenDbTime_UpdatesDbLastWriteTimeUtc()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        DateTime physicalLastWriteTimeUtc = DateTime.Now;
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await Assert.DbLastWriteTimeUtcHasBeenUpdated(physicalLastWriteTimeUtc);
    }

    [Test]
    public async Task ContainerDoesNotExist_PhysicalTimeIsGreaterThenDbTime_UpdatesDbLastWriteTimeUtc()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        DateTime physicalLastWriteTimeUtc = DateTime.Now;
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await Assert.DbLastWriteTimeUtcHasBeenUpdated(physicalLastWriteTimeUtc);
    }

    [Test]
    public async Task ContainerDoesNotExist_NoDbRecords_CreatesDbLastWriteTimeUtc()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        DateTime physicalLastWriteTimeUtc = DateTime.Now;
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(null);

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await _dbContext.Received()
            .CreateLastWriteTimeUtcAsync(_containerVerb.ContainerName, physicalLastWriteTimeUtc);
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
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc);

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await Assert.ContainerHasBeenStarted();
    }

    // --------------------- stopped container, physical time is less

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsLessThenDbTime_UpdatesDbLastWriteTimeUtc()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

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
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await Assert.ContainerHasBeenRemoved();
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsLessThenDbTime_CreatesContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

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
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

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
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

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
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await Assert.ContainerHasBeenRemoved();
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsGreaterThenDbTime_CreatesContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

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
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_containerVerb, CancellationToken.None);

        // Assert:
        await Assert.CreatedContainerHasBeenStarted();
    }


    // --------------------- mock helper classes

    private sealed class SetupMocks : SetupMocksBase<ContainerVerbHandlerTests>
    {
        internal SetupMocks(ContainerVerbHandlerTests testDbOptionsHandlerTests)
            : base(testDbOptionsHandlerTests)
        { }
    }

    private sealed class AssertMocks : AssertMocksBase<ContainerVerbHandlerTests>
    {
        internal AssertMocks(ContainerVerbHandlerTests testDbOptionsHandlerTests)
            : base(testDbOptionsHandlerTests)
        { }
    }
}
