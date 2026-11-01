using System.Net.Http.Json;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Api.Tests;

[Collection("api")]
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


        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 200, 15, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();
        response = await BulkPoolHelper.CreatePoolAsync(client, 150, product.Value, 300, 20, DateTimeOffset.UtcNow.AddDays(60));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        result.Items.Count.Should().Be(2);

        foreach(var pools in result.Items)
        {
            pools.Status.Should().Be(BulkPoolStatus.Open);
        }
        
    }


    [Fact]
    public async Task CreatePool_WithInvalidProduct_ReturnsInternalServer()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 150, 10, DateTimeOffset.UtcNow.AddDays(30));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreatePool_WithInvalidDeadline_ReturnsBadRequest()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 150, 10, DateTimeOffset.UtcNow.AddDays(-30));
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JoinPool_WithValidMerchant_ReturnsSuccessAndParticipantAddedAndQuantityIncremented()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 150, 10, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        result.Items[0].CurrentQuantity.Should().Be(100);
        
    }

    [Fact]
    public async Task JoinPool_TwiceWithSameMerchant_ReturnsBadRequest()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 150, 10, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        var poolId = result.Items[0].PoolId; 

        var joinPool = await client.PostAsJsonAsync($"/api/bulkpools/{poolId}/join", new
        {
            quantity = 150
        });

        joinPool.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        
    }

    [Fact]
    public async Task LeavePool_WithValidMerchant_ShouldBeRemoved()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 150, 10, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        result.Items[0].CurrentQuantity.Should().Be(100);
        var poolId = result.Items[0].PoolId; 

        var leavePool = await client.PostAsync($"/api/bulkpools/{poolId}/leave", null);
        leavePool.EnsureSuccessStatusCode();

        result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        result.Items.Count.Should().Be(0);
             
    }

    [Fact]
    public async Task LeavePool_WithInvalidMerchant_ReturnsBadRequest()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var invalidClient = await _fixture.CreateAuthenticatedClientAsync("0839954789", "$tr0ngP@ssw0rd");

        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 150, 10, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        var poolId = result.Items[0].PoolId; 

        var leavePool = await invalidClient.PostAsync($"/api/bulkpools/{poolId}/leave", null);
        leavePool.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
             
    }

    [Fact]
    public async Task LockPool_WhenBelowThreshold_ReturnsBadRequest()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 150, 10, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        var poolId = result.Items[0].PoolId; 

        var lockPool = await client.PostAsync($"/api/bulkpools/{poolId}/lock", null);
        lockPool.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
             
    }

    [Fact]
    public async Task LockPool_WhenAtOrAboveThreshold_ShouldUpdateStatusToLocked()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 150, product.Value, 100, 1, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        var poolId = result.Items[0].PoolId; 

        var lockPool = await client.PostAsync($"/api/bulkpools/{poolId}/lock", null);
        lockPool.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
             
        result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        result.Items[0].Status.Should().Be(BulkPoolStatus.Locked);
    }

    [Fact]
    public async Task FinalisePool_WhenLocked_ShouldUpdateStatusToFinalized()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 150, product.Value, 100, 1, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        var poolId = result.Items[0].PoolId; 

        var lockPool = await client.PostAsync($"/api/bulkpools/{poolId}/lock", null);         
        lockPool.EnsureSuccessStatusCode();

        var finalisePool = await client.PostAsync($"/api/bulkpools/{poolId}/finalise", null);
        finalisePool.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        result.Items[0].Status.Should().Be(BulkPoolStatus.Finalized);
    }

    [Fact]
    public async Task FinalisePool_WhenOpen_ReturnBadRequest()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 150, 1, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        var poolId = result.Items[0].PoolId; 

        var finalisePool = await client.PostAsync($"/api/bulkpools/{poolId}/finalise", null);
        finalisePool.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
 
    }

    [Fact]
    public async Task CancelPool_WhenThresholdMetOrBelow_ReturnSuccessMessage()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 100, product.Value, 150, 1, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        var poolId = result.Items[0].PoolId; 

        var cancelPool = await client.PostAsync($"/api/bulkpools/{poolId}/cancel", null);
        cancelPool.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        result.Items[0].Status.Should().Be(BulkPoolStatus.Cancelled);
    }

    [Fact]
    public async Task CancelPool_WhenFinalized_ReturnsBadRequest()
    {
        ProductId product = ProductId.New();
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await BulkPoolHelper.CreatePoolAsync(client, 150, product.Value, 100, 1, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();

        var result = await BulkPoolHelper.GetMyPoolsAsync(client, 1, 10);
        var poolId = result.Items[0].PoolId; 

        var lockPool = await client.PostAsync($"/api/bulkpools/{poolId}/lock", null);         
        lockPool.EnsureSuccessStatusCode();

        var finalisePool = await client.PostAsync($"/api/bulkpools/{poolId}/finalise", null);
        finalisePool.EnsureSuccessStatusCode();

        var cancelPool = await client.PostAsync($"/api/bulkpools/{poolId}/cancel", null);
        cancelPool.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

}