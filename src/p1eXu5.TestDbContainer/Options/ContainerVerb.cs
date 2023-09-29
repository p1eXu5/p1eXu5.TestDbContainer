using CommandLine;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Models;

namespace p1eXu5.TestDbContainer.Options;

[Verb("container", isDefault: true)]
internal sealed record ContainerVerb : TestDbOptionsBase, IMySqlContainerParameters
{
    [Option(shortName: 'e', longName: "external-port", Required = true, HelpText = "Database docker container external port.")]
    public required int ContainerExternalPort { get; init; }

    [Option(shortName: 'n', longName: "db-name", Required = true, HelpText = "Database name.")]
    public required string DatabaseName { get; init; }

    [Option(longName: "--reinit-migration")]
    public bool ReInitMigration { get; init; }

    public string MySqlConnectionString(LocalIP localIP)
        => $"server={localIP.Value};port={ContainerExternalPort};uid=admin;pwd=admin;database={DatabaseName}";
}
