using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Worker.Tests;

/// <summary>One real SQL Server container for all outbox persistence tests. Migrations are applied once.</summary>
public sealed class OutboxSqlFixture : IAsyncLifetime
{
    private MsSqlContainer _container = null!;

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public BulkzaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<BulkzaDbContext>().UseSqlServer(ConnectionString).Options);

    public async Task ClearOutboxAsync()
    {
        await using var db = CreateDbContext();
        await db.Outbox.ExecuteDeleteAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

[CollectionDefinition("outbox-sql")]
public class OutboxSqlCollection : ICollectionFixture<OutboxSqlFixture>;
