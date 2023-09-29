namespace p1eXu5.TestDbContainer.Interfaces;

internal interface IDbContext
{
    Task CreateLastWriteTimeUtcAsync(string migrationAlias, DateTime modifiedDate);

    Task<DateTime?> GetDateModifiedAsync(string migrationAlias);
    
    Task UpdateLastWriteTimeUtcAsync(string migrationAlias, DateTime modifiedDate);
}