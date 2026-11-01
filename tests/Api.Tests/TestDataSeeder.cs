using Domain;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;
public static class TestDataSeeder
{
    public static async Task SeedProductsAsync(ApiSmokeTestFixture fixture, params Product[] products)
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();
        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();
        
    }

    public static async Task SeedPoolsAsync(ApiSmokeTestFixture fixture, params BulkPool[] pools)
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();
        await db.BulkPools.AddRangeAsync(pools);
        await db.SaveChangesAsync();
        
    }
}