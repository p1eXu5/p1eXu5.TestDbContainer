using CommandLine;
using p1eXu5.TestDbContainer.Models;

namespace p1eXu5.TestDbContainer.Options;

[Verb("compose")]
internal sealed record ComposeVerb : TestDbOptionsBase
{
    [Option(shortName: 'y', longName: "yaml-file", Required = true, HelpText = "Docker compose file.")]
    public required string ComposeFile { get; init; }

    [Option(longName: "--reinit-migration")]
    public bool ReInitMigration { get; init; }

    public ComposeFilePath ComposeFilePath => new ComposeFilePath(ComposeFile);
}
