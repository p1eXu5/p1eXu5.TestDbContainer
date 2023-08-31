namespace p1eXu5.TestDbContainer.Interfaces;

internal interface IPhysicalDirectory
{
    bool Exists(string path);
    DateTime LastWriteTimeUtc(string path);
}