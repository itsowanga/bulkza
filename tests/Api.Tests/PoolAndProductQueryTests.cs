using System.Net;
using System.Net.Http.Json;
using Application;
using Application.BulkPools;
using Application.Products.SearchProduct;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Api.Tests;

/// <summary>The read endpoints the web frontend relies on: pool by id, open pools, product by id, string enums.</summary>
[Collection("api")]
public class PoolAndProductQueryTests
{
    private const string PhoneA = "0859954789";
    private const string PhoneB = "0825547878";
    private const string Password = "$tr0ngP@ssw0rd";

    private readonly ApiSmokeTestFixture _fixture;

    public PoolAndProductQueryTests(ApiSmokeTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    private async Task<ProductId> SeedProductAsync(string sku = "sku-12345678910", ProductCategory category = ProductCategory.MaizeMeal)
    {
        var id = ProductId.New();
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(id, sku, "White Star 1kg", null, category,
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true));
        return id;
    }

    private async Task<Guid> CreatePoolAsync(HttpClient client, ProductId product, int quantity, int minimumQuantity, int minimumMerchants)
    {
        var response = await BulkPoolHelper.CreatePoolAsync(client, quantity, product.Value, minimumQuantity, minimumMerchants, DateTimeOffset.UtcNow.AddDays(30));
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatedPool>(TestJson.Options);
        return created!.PoolId;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<T>(TestJson.Options))!;
    }

    // Pool by id

    [Fact]
    public async Task GetPool_ReturnsThresholdsParticipantCountAndMyQuantity()
    {
        var product = await SeedProductAsync();
        var client = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);
        var poolId = await CreatePoolAsync(client, product, quantity: 20, minimumQuantity: 100, minimumMerchants: 3);

        var pool = await ReadAsync<BulkPoolDto>(await client.GetAsync($"/api/bulkpools/{poolId}"));

        pool.PoolId.Should().Be(poolId);
        pool.ProductId.Should().Be(product.Value);
        pool.Status.Should().Be(BulkPoolStatus.Open);
        pool.CurrentQuantity.Should().Be(20);
        pool.MyQuantity.Should().Be(20);
        pool.MinimumQuantity.Should().Be(100);
        pool.MinimumMerchants.Should().Be(3);
        pool.ParticipantCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPool_AsAMerchantWhoHasNotJoined_ShowsZeroMyQuantity()
    {
        var product = await SeedProductAsync();
        var creator = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);
        var poolId = await CreatePoolAsync(creator, product, 20, 100, 3);
        var other = await _fixture.CreateAuthenticatedClientAsync(PhoneB, Password);

        var pool = await ReadAsync<BulkPoolDto>(await other.GetAsync($"/api/bulkpools/{poolId}"));

        pool.MyQuantity.Should().Be(0);
        pool.CurrentQuantity.Should().Be(20);
        pool.ParticipantCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPool_Unknown_ReturnsNotFound()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);

        var response = await client.GetAsync($"/api/bulkpools/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPool_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _fixture.Client.GetAsync($"/api/bulkpools/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Open pools

    [Fact]
    public async Task OpenPools_ListsOnlyOpenPoolsOfTheProduct_ForAnyMerchant_AndTracksMyQuantity()
    {
        var product = await SeedProductAsync("sku-12345678910");
        var otherProduct = await SeedProductAsync("sku-12345678911");
        var creator = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);

        var openPool = await CreatePoolAsync(creator, product, quantity: 20, minimumQuantity: 100, minimumMerchants: 2);
        await CreatePoolAsync(creator, product, quantity: 150, minimumQuantity: 100, minimumMerchants: 1);        // threshold met, no longer open
        await CreatePoolAsync(creator, otherProduct, quantity: 20, minimumQuantity: 100, minimumMerchants: 2);    // different product

        var joiner = await _fixture.CreateAuthenticatedClientAsync(PhoneB, Password);
        var before = await ReadAsync<PagedResult<BulkPoolDto>>(await joiner.GetAsync($"/api/bulkpools/open?productId={product.Value}&page=1&pageSize=10"));

        before.TotalCount.Should().Be(1);
        before.Items.Should().ContainSingle().Which.PoolId.Should().Be(openPool);
        before.Items[0].MyQuantity.Should().Be(0);

        var join = await joiner.PostAsJsonAsync($"/api/bulkpools/{openPool}/join", new { quantity = 5 });
        join.EnsureSuccessStatusCode();

        var after = await ReadAsync<PagedResult<BulkPoolDto>>(await joiner.GetAsync($"/api/bulkpools/open?productId={product.Value}&page=1&pageSize=10"));
        after.Items.Single().MyQuantity.Should().Be(5);
        after.Items.Single().CurrentQuantity.Should().Be(25);
        after.Items.Single().ParticipantCount.Should().Be(2);
    }

    [Fact]
    public async Task OpenPools_ForAProductWithNoPools_ReturnsAnEmptyPage()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);

        var page = await ReadAsync<PagedResult<BulkPoolDto>>(await client.GetAsync($"/api/bulkpools/open?productId={Guid.NewGuid()}&page=1&pageSize=10"));

        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task OpenPools_WithInvalidPaging_ReturnsBadRequest(int page, int pageSize)
    {
        var client = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);

        var response = await client.GetAsync($"/api/bulkpools/open?productId={Guid.NewGuid()}&page={page}&pageSize={pageSize}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Product by id

    [Fact]
    public async Task GetProduct_ReturnsCategoryUnitAndTheDiscountLadderInAscendingOrder()
    {
        var id = await SeedProductAsync(category: ProductCategory.Rice);
        var client = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);

        var product = await ReadAsync<ProductDto>(await client.GetAsync($"/api/products/{id.Value}"));

        product.ProductId.Should().Be(id.Value);
        product.Name.Should().Be("White Star 1kg");
        product.BasePrice.Should().Be(59.99m);
        product.Category.Should().Be(ProductCategory.Rice);
        product.UnitOfMeasure.Should().Be(UnitOfMeasure.Kilogram);
        product.DiscountTiers.Select(t => (t.MinimumQuantity, t.DiscountPercentage)).Should().Equal(
            (1, 0m), (50, 5m), (100, 10m), (250, 15m), (500, 20m), (1000, 25m));
    }

    [Fact]
    public async Task GetProduct_Unknown_ReturnsNotFound()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BrowseProducts_AcceptsACategoryName()
    {
        await SeedProductAsync(category: ProductCategory.Rice);
        var client = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);

        var rice = await ReadAsync<PagedResult<ProductDto>>(await client.GetAsync("/api/products/browse?page=1&pageSize=10&category=Rice"));
        var sugar = await ReadAsync<PagedResult<ProductDto>>(await client.GetAsync("/api/products/browse?page=1&pageSize=10&category=Sugar"));

        rice.Items.Should().ContainSingle();
        sugar.Items.Should().BeEmpty();
    }

    // Enums travel as strings

    [Fact]
    public async Task Enums_AreWrittenAsReadableStrings_NotNumbers()
    {
        var product = await SeedProductAsync(category: ProductCategory.CookingOil);
        var client = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);
        var poolId = await CreatePoolAsync(client, product, 20, 100, 2);

        var poolJson = await (await client.GetAsync($"/api/bulkpools/{poolId}")).Content.ReadAsStringAsync();
        var productJson = await (await client.GetAsync($"/api/products/{product.Value}")).Content.ReadAsStringAsync();

        poolJson.Should().Contain("\"status\":\"Open\"");
        productJson.Should().Contain("\"category\":\"CookingOil\"").And.Contain("\"unitOfMeasure\":\"Kilogram\"");
    }

    private sealed record CreatedPool(Guid PoolId, BulkPoolStatus Status, DateTimeOffset CreatedAt);
}
