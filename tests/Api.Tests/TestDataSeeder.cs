using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
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

    /// <summary>
    /// Inserts an active Admin merchant straight into the database. Registration always creates plain merchants,
    /// so this is the only way a test gets an admin. Log in after seeding so the JWT carries the Admin role claim.
    /// </summary>
    public static async Task<(string Phone, string Password)> SeedAdminAsync(ApiSmokeTestFixture fixture,
                                                                             string phone = "0825550001",
                                                                             string password = "Adm1n!Passw0rd")
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var admin = new Merchant(MerchantId.New(), "Test Admin", PasswordHash.Create(hasher.Hash(password)),
                                 MerchantStatus.Active, MerchantRole.Admin,
                                 Address.Create("1 Admin Road", "Pretoria", "Gauteng", "0001", "South Africa"),
                                 PhoneNumber.Create(phone), DateTimeOffset.UtcNow);

        db.Merchants.Add(admin);
        await db.SaveChangesAsync();

        return (phone, password);
    }

    public static async Task SeedPoolsAsync(ApiSmokeTestFixture fixture, params BulkPool[] pools)
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();
        await db.BulkPools.AddRangeAsync(pools);
        await db.SaveChangesAsync();
        
    }
}