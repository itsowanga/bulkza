using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;
public class BulkzaDbContextFactory : IDesignTimeDbContextFactory<BulkzaDbContext>
{
    public BulkzaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BulkzaDbContext>()
        .UseSqlServer("Server=localhost;Database=MyApp;Trusted_Connection=True;").Options;

        return new BulkzaDbContext(options);
    }
}