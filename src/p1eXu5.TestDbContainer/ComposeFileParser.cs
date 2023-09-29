using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace p1eXu5.TestDbContainer;

internal sealed class ComposeFileParser : IComposeFileParser
{
    public bool FileExists(ComposeFilePath migrationFolderPath)
    {
        return File.Exists(migrationFolderPath.Path);
    }

    public ComposeFile Parse(ComposeFilePath composeFilePath)
    {
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
            .Build();

        using var sr = File.OpenText(composeFilePath.Path);

        return deserializer.Deserialize<ComposeFile>(sr);
    }
}
