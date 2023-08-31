using CommandLine;
using p1eXu5.CliBootstrap.CommandLineParser.Options;

namespace TestDbContainer;

public sealed record TestDbOptions : CommonOptions
{
    [Option(shortName: 'm', longName: "migrations", Required = true, HelpText = "Migrations folder path")]
    public required string MigrationPath { get; init; }

    [Option(shortName: 'p', longName: "project", Required = true, HelpText = "The project to use. Defaults to the current working directory.")]
    public required string MigrationProjectPath { get; init; }

    [Option(shortName: 's', longName: "startup-project", Required = true, HelpText = "The startup project to use. Defaults to the current working directory.")]
    public required string StartupProjectPath { get; init; }

    [Option(shortName: 'c', longName: "container-name", Required = true, HelpText = "Database docker container name.")]
    public required string ContainerName { get; init; }

    [Option(shortName: 'e', longName: "external-port", Required = true, HelpText = "Database docker container external port.")]
    public required int ContainerExternalPort { get; init; }

    [Option(shortName: 'n', longName: "db-name", Required = true, HelpText = "Database name.")]
    public required string DatabaseName { get; init; }

    public static TestDbOptions CoreDomainTestDb { get; } =
        new TestDbOptions
        {
            MigrationPath = Paths.CoreDomainMigrations,
            MigrationProjectPath = Paths.CoreDomainMigrationsProject,
            StartupProjectPath = Paths.CoreDomainStartupProject,
            ContainerName = "test-core-domain-db",
            ContainerExternalPort = 3367,
            DatabaseName = "drug_room"
        };
}
