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
        _connectionString = $"Data Source={Paths.Dbs}";
    }

    public static DbContext Instance => _instance ??= new();

    public async Task InitAsync()
    {
        using var connection = GetDbConnection();
        var sql =
            """
                CREATE TABLE IF NOT EXISTS 
                Migrations (
                    Name TEXT NOT NULL PRIMARY KEY,
                    LastWriteTimeUtc TEXT NOT NULL
                );
            """;
        await connection.ExecuteAsync(sql);
    }

    public async Task<DateTime?> GetDateModifiedAsync(string name)
    {
        using var connection = GetDbConnection();
        var sql =
            $"""
            SELECT LastWriteTimeUtc FROM Migrations WHERE Name LIKE '{name}';
            """;

        string? dateModifiedStr = await connection.QuerySingleOrDefaultAsync<string?>(sql);
        return dateModifiedStr is null ? null : DateTime.Parse(dateModifiedStr);
    }

    public async Task UpdateLastWriteTimeUtcAsync(string containerName, DateTime modifiedDate)
    {
        using var connection = GetDbConnection();
        var sql =
            $"""
            UPDATE Migrations SET LastWriteTimeUtc = '{modifiedDate:yyyy-MM-dd HH:mm:ss.fff}'
            WHERE Name LIKE '{containerName}';
            """;

        await connection.ExecuteAsync(sql);
    }

    public async Task CreateLastWriteTimeUtcAsync(string containerName, DateTime modifiedDate)
    {
        using var connection = GetDbConnection();
        var sql =
            $"""
            INSERT INTO Migrations (Name, LastWriteTimeUtc)
            VALUES ('{containerName}', '{modifiedDate:yyyy-MM-dd HH:mm:ss.fff}');
            """;

        await connection.ExecuteAsync(sql);
    }

    private IDbConnection GetDbConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
