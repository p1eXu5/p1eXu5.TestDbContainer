using p1eXu5.TestDbContainer.Models;

namespace p1eXu5.TestDbContainer.Interfaces;

internal interface IMigrationFolder
{
    bool Exists(MigrationFolderPath migrationFolderPath);

    bool IsEmpty(MigrationFolderPath migrationFolderPath);

    DateTime LastWriteTimeUtc(MigrationFolderPath migrationFolderPath);
}