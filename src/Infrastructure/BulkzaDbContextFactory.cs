using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;

/// <summary>
/// Used only by the EF Core command line tools (dotnet ef). It holds no credentials.
/// Commands that need a real database, such as "database update", must be given one with
/// --connection or the ConnectionStrings__DefaultConnection environment variable.
/// Commands that only read the model, such as "migrations add", need no connection.
/// </summary>
public class BulkzaDbContextFactory : IDesignTimeDbContextFactory<BulkzaDbContext>
{
    private const string EnvironmentVariable = "ConnectionStrings__DefaultConnection";

    // Integrated security, no password. It lets the model build when no connection string is supplied,
    // and it cannot connect to anything useful by accident.
    private const string ModelOnlyConnectionString = "Server=localhost;Database=bulkza_design_time;Integrated Security=true;TrustServerCertificate=True";

    public BulkzaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString)) connectionString = ModelOnlyConnectionString;

        var options = new DbContextOptionsBuilder<BulkzaDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BulkzaDbContext(options);
    }
}
