using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;
public class BulkzaDbContextFactory : IDesignTimeDbContextFactory<BulkzaDbContext>
{
    public BulkzaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BulkzaDbContext>()
        .UseSqlServer("Server=10.0.0.106,1433;Database=bulkza;User Id=sa;Password=HelloSowangaM050220??;TrustServerCertificate=True").Options;

        return new BulkzaDbContext(options);
    }
}