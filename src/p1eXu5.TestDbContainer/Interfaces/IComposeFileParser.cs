using p1eXu5.TestDbContainer.Models;

namespace p1eXu5.TestDbContainer.Interfaces;

internal interface IComposeFileParser
{
    bool FileExists(ComposeFilePath migrationFolderPath);

    ComposeFile Parse(ComposeFilePath composeFilePath);
}
