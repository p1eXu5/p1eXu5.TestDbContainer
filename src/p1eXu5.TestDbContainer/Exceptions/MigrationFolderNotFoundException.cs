namespace p1eXu5.TestDbContainer.Exceptions;

public class MigrationFolderNotFoundException : DirectoryNotFoundException
{
    public MigrationFolderNotFoundException()
    {
    }

    public MigrationFolderNotFoundException(string message) : base(message)
    {
    }

    public MigrationFolderNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
