using AutoBogus;
using Microsoft.Extensions.Logging;
using p1eXu5.TestDbContainer.Handlers;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Models;
using p1eXu5.TestDbContainer.Options;

namespace p1eXu5.TestDbContainer.Tests.Unit.Handlers;

public sealed class ComposeVerbHandlerTests : VerbHandlerTestsBase
{
    private ComposeVerbHandler _sut;

    private IComposeFileParser _composeFileParser;
    private ComposeVerb _composeVerb;

    private SetupMocks Setup { get; set; }

    private AssertMocks Assert { get; set; }

    [SetUp]
    public void Initialize()
    {
        _composeVerb = AutoFaker.Generate<ComposeVerb>();

        _composeFileParser = Substitute.For<IComposeFileParser>();
        _composeFileParser.FileExists(default!).ReturnsForAnyArgs(true);

        Setup = new SetupMocks(this, _composeVerb);
        Assert = new AssertMocks(this);

        var logger = Substitute.For<ILogger>();

        _sut = new ComposeVerbHandler(
            _migrationFolder,
            _dbContext,
            _dockerContainer,
            _composeFileParser,
            _dotnetCli,
            logger);


    }

    [Test]
    public async Task ComposeFileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange:
        Setup.ComposeFileDoesNotExist();

        // Action:
        var action = () => _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        await action.Should().ThrowAsync<FileNotFoundException>();
    }

    [Test]
    public async Task MigrationFolderDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // Arrange:
        Setup.MigrationFolderDoesNotExist();

        // Action:
        var action = () => _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        await action.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Test]
    public async Task MigrationFolderIsEmpty_CallsCreateInitMigration()
    {
        // Arrange:
        Setup.MigrationFolderIsEmpty();

        // Action:
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        Assert.InitMigrationHasBeenCreated();
    }

    // -------------------- container does not exist

    [Test]
    public async Task ContainerDoesNotExist_ComposesContainer()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        // Action:
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        Assert.ContainerHasBeenComposed(_composeVerb);
    }

    [Test]
    public async Task ContainerDoesNotExist_UpdatesDatabase()
    {
        // Arrange:
        Setup.ContainerDoesNotExist();

        // Action:
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        Assert.DatabaseHasBeenUpdated(_composeVerb);
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
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

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
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

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
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        await _dbContext.Received()
            .CreateLastWriteTimeUtcAsync(_sut.GetMigrationAlias(_composeVerb), physicalLastWriteTimeUtc);
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
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

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
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

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
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        await Assert.ContainerHasBeenRemoved();
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsLessThenDbTime_ComposesContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc + TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        Assert.ContainerHasBeenComposed(_composeVerb);
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
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

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
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        await Assert.ContainerHasBeenRemoved();
    }

    [Test]
    public async Task ContainerExistsAndStopped_PhysicalTimeIsGreaterThenDbTime_ComposesContainer()
    {
        // Arrange:
        var physicalLastWriteTimeUtc = DateTime.Now;
        Setup.ContainerExists();
        Setup.ContainerIsStopped();
        Setup.DbLastWriteTimeUtc(physicalLastWriteTimeUtc);
        Setup.MigrationFolderLastWriteTimeUtc(physicalLastWriteTimeUtc - TimeSpan.FromMinutes(1));

        // Action:
        await _sut.ProcessAsync(_composeVerb, CancellationToken.None);

        // Assert:
        Assert.ContainerHasBeenComposed(_composeVerb);
    }

    // --------------------- mock helper classes

    private sealed class SetupMocks : SetupMocksBase<ComposeVerbHandlerTests>
    {
        internal SetupMocks(ComposeVerbHandlerTests testDbOptionsHandlerTests, ComposeVerb composeVerb)
            : base(testDbOptionsHandlerTests)
        {
            _._composeFileParser.Parse(default!)
                .ReturnsForAnyArgs(new ComposeFile()
                {
                    Name = "test-yaml",
                    Services = new Dictionary<string, Service>
                    {
                        [composeVerb.ContainerName] = new Service
                        {
                            Environment = new Dictionary<string, string>
                            {
                                ["MYSQL_ROOT_PASSWORD"] = "admin",
                                ["MYSQL_DATABASE"] = "test_database_name",
                                ["MYSQL_USER"] = "admin",
                                ["MYSQL_PASSWORD"] = "admin",
                            },
                            Ports = new System.Collections.ObjectModel.Collection<string> { "3377:3306" },
                        },
                    }
                });
        }

        internal void ComposeFileDoesNotExist()
            => _._composeFileParser.FileExists(default!).ReturnsForAnyArgs(false);
    }

    private sealed class AssertMocks : AssertMocksBase<ComposeVerbHandlerTests>
    {
        internal AssertMocks(ComposeVerbHandlerTests testDbOptionsHandlerTests)
            : base(testDbOptionsHandlerTests)
        { }
    }
}
