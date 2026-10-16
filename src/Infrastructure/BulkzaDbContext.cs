using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;
public class BulkzaDbContext(DbContextOptions<BulkzaDbContext> options) : DbContext(options)
{
    public DbSet<Merchant> Merchants {get; set;}
    public DbSet<Product> Products {get; set;}
    public DbSet<BulkPool> BulkPools {get; set;}
    public DbSet<Order> Orders {get; set;}
    public DbSet<Cart> Carts {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BulkzaDbContext).Assembly);
    }
}