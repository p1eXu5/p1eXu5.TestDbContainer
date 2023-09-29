using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using p1eXu5.TestDbContainer.Interfaces;

namespace TestDbContainer;

internal sealed class DbContext : IDbContext
{
    private static DbContext? _instance;
    private readonly string _connectionString;

    private DbContext()
    {
        var testDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test-db");
        if (!Directory.Exists(testDbPath))
        {
            Directory.CreateDirectory(testDbPath);
        }

        var dbPath = Path.Combine(testDbPath, ".dbs");
        _connectionString = $"Data Source={dbPath}";
    }

    public static DbContext Instance => _instance ??= new();

    public async Task InitAsync()
    {
        using var connection = GetDbConnection();
        var sql =
            """
                CREATE TABLE IF NOT EXISTS 
                Migration (
                    Alias TEXT NOT NULL PRIMARY KEY,
                    LastWriteTimeUtc TEXT NOT NULL
                );
            """;
        await connection.ExecuteAsync(sql).ConfigureAwait(false);
    }

    public async Task<DateTime?> GetDateModifiedAsync(string migrationAlias)
    {
        using var connection = GetDbConnection();
        var sql =
            $"""
            SELECT LastWriteTimeUtc FROM Migration WHERE Alias LIKE '{migrationAlias}';
            """;

        string? dateModifiedStr = await connection.QuerySingleOrDefaultAsync<string?>(sql).ConfigureAwait(false);
        return dateModifiedStr is null ? null : DateTime.Parse(dateModifiedStr, null);
    }

    public async Task UpdateLastWriteTimeUtcAsync(string migrationAlias, DateTime modifiedDate)
    {
        using var connection = GetDbConnection();
        var sql =
            $"""
            UPDATE Migration SET LastWriteTimeUtc = '{modifiedDate:yyyy-MM-dd HH:mm:ss.fff}'
            WHERE Alias LIKE '{migrationAlias}';
            """;

        await connection.ExecuteAsync(sql).ConfigureAwait(false);
    }

    public async Task CreateLastWriteTimeUtcAsync(string migrationAlias, DateTime modifiedDate)
    {
        using var connection = GetDbConnection();
        var sql =
            $"""
            INSERT INTO Migration (Alias, LastWriteTimeUtc)
            VALUES ('{migrationAlias}', '{modifiedDate:yyyy-MM-dd HH:mm:ss.fff}');
            """;

        await connection.ExecuteAsync(sql).ConfigureAwait(false);
    }

    private SqliteConnection GetDbConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
