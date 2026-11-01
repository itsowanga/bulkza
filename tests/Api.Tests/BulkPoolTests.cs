using System.Net.Http.Json;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Api.Tests;

//         await TestDataSeeder.SeedPoolsAsync(_fixture, new BulkPool(PoolId.Create(Guid.NewGuid()), productId,
//                                                      Quantity.Create(150), 5, DateTimeOffset.Now.AddDays(10)));
public class BulkPoolTests
{
    private readonly ApiSmokeTestFixture _fixture;

    public BulkPoolTests(ApiSmokeTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetMyPools_WithInvalidPagination_ReturnsBadRequest()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        var response = await client.GetAsync("/api/bulkpools/mine?page=0&pageSize=100");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePool_WithValidFields_ReturnsSuccessMessageWithOpenStatus()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );


        var response = await client.PostAsJsonAsync("/api/bulkpools", new
        {
            productId = product.Value,
            minimumQuantity = 150,
            mininumMerchants = 10,
            deadline = DateTimeOffset.UtcNow.AddDays(30)
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
    }


}