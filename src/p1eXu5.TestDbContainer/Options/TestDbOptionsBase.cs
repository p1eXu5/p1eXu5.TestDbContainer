using CommandLine;
using p1eXu5.CliBootstrap.CommandLineParser.Options;
using p1eXu5.TestDbContainer.Models;
using TestDbContainer;

namespace p1eXu5.TestDbContainer.Options;

internal abstract record TestDbOptionsBase : CliOptions
{
    [Option(shortName: 'c', longName: "container-name", Required = true, HelpText = "Database docker container name.")]
    public required string ContainerName { get; init; }

    [Option(shortName: 'p', longName: "project", Required = true, HelpText = "The project to use.")]
    public required string ProjectPath { get; init; }

    [Option(shortName: 'm', longName: "migrations", HelpText = "Migrations folder path.")]
    public string? MigrationPath { internal get; init; }

    [Option(shortName: 's', longName: "startup-project", HelpText = "The startup project to use.")]
    public string? StartupProjectPath { get; init; }

    [Option(longName: "verbose", HelpText = "Verbose log level.")]
    public bool Verbose { get; init; }

    public MigrationFolderPath MigrationFolderPath
    {
        get
        {
            ArgumentNullException.ThrowIfNull(MigrationPath, nameof(MigrationPath));
            return new MigrationFolderPath(MigrationPath);
        }
    }

    public bool MigrationPathIsSet => MigrationPath is not null;

    public bool StartupProjectPathIsSet => StartupProjectPath is not null;

    /*
    public static TestDbOptionsBase CoreDomainTestDb { get; } =
        new TestDbOptionsBase
        {
            MigrationPath = Paths.CoreDomainMigrations,
            MigrationProjectPath = Paths.CoreDomainMigrationsProject,
            StartupProjectPath = Paths.CoreDomainStartupProject,
            ContainerName = "test-core-domain-db",
            ContainerExternalPort = 3367,
            DatabaseName = "drug_room"
        };
    */
}
