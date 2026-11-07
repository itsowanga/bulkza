using System.Net;
using System.Net.Http.Json;
using Application;
using Application.Admin;
using Application.BulkPools;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Api.Tests;

/// <summary>The admin dashboard's API: read-only views across every merchant, and the ship and deliver steps.</summary>
[Collection("api")]
public class AdminEndpointTests
{
    private const string PhoneA = "0859954789";
    private const string PhoneB = "0825547878";
    private const string Password = "$tr0ngP@ssw0rd";

    private readonly ApiSmokeTestFixture _fixture;

    public AdminEndpointTests(ApiSmokeTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    private async Task<HttpClient> AdminAsync()
    {
        var (phone, password) = await TestDataSeeder.SeedAdminAsync(_fixture);
        return await _fixture.CreateAuthenticatedClientAsync(phone, password);
    }

    private async Task<ProductId> SeedProductAsync()
    {
        var id = ProductId.New();
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(id, "sku-12345678910", "White Star 1kg", null,
                                                                    ProductCategory.MaizeMeal, UnitOfMeasure.Kilogram, Money.Create(59.99m), true));
        return id;
    }

    private static async Task<Guid> CreatePoolAsync(HttpClient client, ProductId product, int quantity, int minimumQuantity, int minimumMerchants)
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

    // Access control

    [Theory]
    [InlineData("/api/admin/pools?page=1&pageSize=10")]
    [InlineData("/api/admin/orders?page=1&pageSize=10")]
    [InlineData("/api/admin/summary")]
    public async Task AdminRoutes_ForAnOrdinaryMerchant_ReturnForbidden(string path)
    {
        var merchant = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);

        var response = await merchant.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/api/admin/pools?page=1&pageSize=10")]
    [InlineData("/api/admin/orders?page=1&pageSize=10")]
    [InlineData("/api/admin/summary")]
    public async Task AdminRoutes_WithoutAToken_ReturnUnauthorized(string path)
    {
        var response = await _fixture.Client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Pools

    [Fact]
    public async Task ListPools_ShowsEveryMerchantsPools_AndFiltersByStatus()
    {
        var product = await SeedProductAsync();
        var a = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);
        var b = await _fixture.CreateAuthenticatedClientAsync(PhoneB, Password);
        var openPool = await CreatePoolAsync(a, product, quantity: 20, minimumQuantity: 100, minimumMerchants: 2);
        var metPool = await CreatePoolAsync(b, product, quantity: 150, minimumQuantity: 100, minimumMerchants: 1);
        var admin = await AdminAsync();

        var all = await ReadAsync<PagedResult<BulkPoolDto>>(await admin.GetAsync("/api/admin/pools?page=1&pageSize=10"));
        all.TotalCount.Should().Be(2);
        all.Items.Select(pool => pool.PoolId).Should().BeEquivalentTo([openPool, metPool]);
        all.Items.Should().OnlyContain(pool => pool.MyQuantity == 0, "the admin has joined neither pool");

        var met = await ReadAsync<PagedResult<BulkPoolDto>>(await admin.GetAsync("/api/admin/pools?status=ThresholdMet&page=1&pageSize=10"));
        met.Items.Should().ContainSingle().Which.PoolId.Should().Be(metPool);

        var open = await ReadAsync<PagedResult<BulkPoolDto>>(await admin.GetAsync("/api/admin/pools?status=Open&page=1&pageSize=10"));
        open.Items.Should().ContainSingle().Which.PoolId.Should().Be(openPool);
    }

    [Theory]
    [InlineData("page=0&pageSize=10")]
    [InlineData("page=1&pageSize=0")]
    [InlineData("page=1&pageSize=51")]
    [InlineData("status=NotAStatus&page=1&pageSize=10")]
    public async Task ListPools_WithInvalidInput_ReturnsBadRequest(string query)
    {
        var admin = await AdminAsync();

        var response = await admin.GetAsync($"/api/admin/pools?{query}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Orders, ship and deliver

    [Fact]
    public async Task ListOrders_NamesTheMerchant_AndFollowsAnOrderThroughShipAndDeliver()
    {
        var product = await SeedProductAsync();
        var merchant = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);
        var poolId = await CreatePoolAsync(merchant, product, quantity: 150, minimumQuantity: 100, minimumMerchants: 1);
        var admin = await AdminAsync();
        (await admin.PostAsync($"/api/bulkpools/{poolId}/lock", null)).EnsureSuccessStatusCode();
        (await admin.PostAsync($"/api/bulkpools/{poolId}/finalise", null)).EnsureSuccessStatusCode();

        var pending = await ReadAsync<PagedResult<AdminOrderDto>>(await admin.GetAsync("/api/admin/orders?status=Pending&page=1&pageSize=10"));
        var order = pending.Items.Should().ContainSingle().Subject;
        order.MerchantName.Should().Be("Test Merchant");
        order.OrderLines.Should().ContainSingle().Which.Quantity.Should().Be(150);
        order.TotalAmount.Should().Be(53.99m * 150);

        (await merchant.PostAsJsonAsync($"/api/orders/{order.OrderId}/pay", new { cardToken = "tok_test_visa_4242" })).EnsureSuccessStatusCode();
        (await ReadAsync<PagedResult<AdminOrderDto>>(await admin.GetAsync("/api/admin/orders?status=Pending&page=1&pageSize=10"))).Items.Should().BeEmpty();
        var paid = await ReadAsync<PagedResult<AdminOrderDto>>(await admin.GetAsync("/api/admin/orders?status=Paid&page=1&pageSize=10"));
        paid.Items.Should().ContainSingle().Which.OrderId.Should().Be(order.OrderId);

        (await admin.PostAsync($"/api/orders/{order.OrderId}/ship", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var shipped = await ReadAsync<PagedResult<AdminOrderDto>>(await admin.GetAsync("/api/admin/orders?status=Shipped&page=1&pageSize=10"));
        shipped.Items.Should().ContainSingle().Which.OrderStatus.Should().Be(OrderStatus.Shipped);

        (await admin.PostAsync($"/api/orders/{order.OrderId}/deliver", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var everything = await ReadAsync<PagedResult<AdminOrderDto>>(await admin.GetAsync("/api/admin/orders?page=1&pageSize=10"));
        everything.Items.Should().ContainSingle().Which.OrderStatus.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public async Task ListOrders_ShowsOrdersOfEveryMerchant()
    {
        var product = await SeedProductAsync();
        var a = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);
        var poolId = await CreatePoolAsync(a, product, quantity: 60, minimumQuantity: 100, minimumMerchants: 2);
        var b = await _fixture.CreateAuthenticatedClientAsync(PhoneB, Password);
        (await b.PostAsJsonAsync($"/api/bulkpools/{poolId}/join", new { quantity = 60 })).EnsureSuccessStatusCode();
        var admin = await AdminAsync();
        (await admin.PostAsync($"/api/bulkpools/{poolId}/lock", null)).EnsureSuccessStatusCode();
        (await admin.PostAsync($"/api/bulkpools/{poolId}/finalise", null)).EnsureSuccessStatusCode();

        var orders = await ReadAsync<PagedResult<AdminOrderDto>>(await admin.GetAsync("/api/admin/orders?page=1&pageSize=10"));

        orders.TotalCount.Should().Be(2);
        orders.Items.Select(order => order.MerchantId).Distinct().Should().HaveCount(2);
        orders.Items.Should().OnlyContain(order => order.OrderStatus == OrderStatus.Pending);
    }

    [Theory]
    [InlineData("page=0&pageSize=10")]
    [InlineData("page=1&pageSize=0")]
    [InlineData("status=NotAStatus&page=1&pageSize=10")]
    public async Task ListOrders_WithInvalidInput_ReturnsBadRequest(string query)
    {
        var admin = await AdminAsync();

        var response = await admin.GetAsync($"/api/admin/orders?{query}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Summary

    [Fact]
    public async Task Summary_CountsPoolsAndOrdersByStatus_AndIncludesEveryStatus()
    {
        var product = await SeedProductAsync();
        var a = await _fixture.CreateAuthenticatedClientAsync(PhoneA, Password);
        var b = await _fixture.CreateAuthenticatedClientAsync(PhoneB, Password);
        await CreatePoolAsync(a, product, quantity: 20, minimumQuantity: 100, minimumMerchants: 2);                      // stays Open
        var finalised = await CreatePoolAsync(b, product, quantity: 150, minimumQuantity: 100, minimumMerchants: 1);
        var admin = await AdminAsync();
        (await admin.PostAsync($"/api/bulkpools/{finalised}/lock", null)).EnsureSuccessStatusCode();
        (await admin.PostAsync($"/api/bulkpools/{finalised}/finalise", null)).EnsureSuccessStatusCode();

        var summary = await ReadAsync<AdminSummaryDto>(await admin.GetAsync("/api/admin/summary"));

        summary.Pools[BulkPoolStatus.Open].Should().Be(1);
        summary.Pools[BulkPoolStatus.Finalized].Should().Be(1);
        summary.Pools[BulkPoolStatus.Locked].Should().Be(0);
        summary.Pools.Keys.Should().BeEquivalentTo(Enum.GetValues<BulkPoolStatus>());
        summary.Orders[OrderStatus.Pending].Should().Be(1);
        summary.Orders[OrderStatus.Paid].Should().Be(0);
        summary.Orders.Keys.Should().BeEquivalentTo(Enum.GetValues<OrderStatus>());
    }

    private sealed record CreatedPool(Guid PoolId, BulkPoolStatus Status, DateTimeOffset CreatedAt);
}
