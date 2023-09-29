using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Models;

namespace TestDbContainer;

internal sealed class MigrationFolder : IMigrationFolder
{
    public bool Exists(MigrationFolderPath migrationFolderPath)
    {
        return Directory.Exists(migrationFolderPath.Path);
    }

    public bool IsEmpty(MigrationFolderPath migrationFolderPath)
    {
        return !Directory.EnumerateFileSystemEntries(migrationFolderPath.Path).Any();
    }

    public DateTime LastWriteTimeUtc(MigrationFolderPath migrationFolderPath)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(migrationFolderPath.Path);
        return directoryInfo.LastWriteTimeUtc;
    }
}
