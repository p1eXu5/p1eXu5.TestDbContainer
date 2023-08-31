namespace p1eXu5.TestDbContainer.Interfaces;

internal interface IDbContext
{
    Task CreateLastWriteTimeUtcAsync(string containerName, DateTime modifiedDate);
    Task<DateTime?> GetDateModifiedAsync(string name);
    Task UpdateLastWriteTimeUtcAsync(string containerName, DateTime modifiedDate);
}