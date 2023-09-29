using CommandLine;
using p1eXu5.CliBootstrap.CommandLineParser.Options;
using p1eXu5.TestDbContainer.Models;
using TestDbContainer;

namespace p1eXu5.TestDbContainer.Options;

internal abstract record TestDbOptionsBase : CliOptions
{
    [Option(shortName: 'c', longName: "container-name", Required = true, HelpText = "Database docker container name.")]
    public required string ContainerName { get; init; }

    [Option(shortName: 'm', longName: "migrations", Required = true, HelpText = "Migrations folder path.")]
    public required string MigrationPath { private get; init; }

    [Option(shortName: 'p', longName: "project", Required = true, HelpText = "The project to use.")]
    public required string MigrationProjectPath { get; init; }

    [Option(shortName: 's', longName: "startup-project", Required = true, HelpText = "The startup project to use.")]
    public required string StartupProjectPath { get; init; }

    [Option(longName: "verbose", HelpText = "Verbose log level.")]
    public bool Verbose { get; init; }

    public MigrationFolderPath MigrationFolderPath => new MigrationFolderPath(MigrationPath);

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
