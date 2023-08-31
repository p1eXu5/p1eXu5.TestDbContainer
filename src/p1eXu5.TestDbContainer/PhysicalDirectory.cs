using p1eXu5.TestDbContainer.Interfaces;

namespace TestDbContainer;

internal sealed class PhysicalDirectory : IPhysicalDirectory
{
    public bool Exists(string path)
    {
        return Directory.Exists(path);
    }

    public DateTime LastWriteTimeUtc(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        return directoryInfo.LastWriteTimeUtc;
    }
}
