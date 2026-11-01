using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;
public static class TestDataSeeder
{
    public static async Task SeedProductsAsync(ApiSmokeTestFixture fixture, params Product[] products)
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();
        Console.WriteLine($"Seeder connection: {db.Database.GetConnectionString()}");
        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();
        
    }
}