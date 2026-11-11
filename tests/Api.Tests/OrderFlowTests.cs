using System.Net;
using System.Net.Http.Json;
using Application;
using Application.BulkPools;
using Application.Orders;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Api.Tests;

/// <summary>Orders end to end: a pool is finalised by an admin, the participant pays or cancels their order.</summary>
[Collection("api")]
public class OrderFlowTests
{
    private const string Phone = "0859954789";
    private const string OtherPhone = "0825547878";
    private const string Password = "$tr0ngP@ssw0rd";

    private readonly ApiSmokeTestFixture _fixture;

    public OrderFlowTests(ApiSmokeTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    private async Task<HttpClient> AdminClientAsync()
    {
        var (phone, password) = await TestDataSeeder.SeedAdminAsync(_fixture);
        return await _fixture.CreateAuthenticatedClientAsync(phone, password);
    }

    /// <summary>Creates a pool of 150 units (above its minimum of 100, so it is ThresholdMet), then locks and finalises it.</summary>
    private async Task<(HttpClient Merchant, Guid ProductId)> FinalisedPoolAsync()
    {
        var product = ProductId.New();
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(product, "sku-12345678910", "White Star 1kg", null,
                                                                    ProductCategory.MaizeMeal, UnitOfMeasure.Kilogram, Money.Create(59.99m), true));
        var merchant = await _fixture.CreateAuthenticatedClientAsync(Phone, Password);
        var created = await BulkPoolHelper.CreatePoolAsync(merchant, 150, product.Value, 100, 1, DateTimeOffset.UtcNow.AddDays(30));
        created.EnsureSuccessStatusCode();
        var poolId = (await BulkPoolHelper.GetMyPoolsAsync(merchant, 1, 10)).Items[0].PoolId;

        var admin = await AdminClientAsync();
        (await admin.PostAsync($"/api/bulkpools/{poolId}/lock", null)).EnsureSuccessStatusCode();
        (await admin.PostAsync($"/api/bulkpools/{poolId}/finalise", null)).EnsureSuccessStatusCode();
        return (merchant, product.Value);
    }

    private static async Task<PagedResult<OrderDto>> MyOrdersAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/orders/mine?page=1&pageSize=20");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<PagedResult<OrderDto>>(TestJson.Options))!;
    }

    [Fact]
    public async Task MyOrders_WithNoOrders_ReturnsAnEmptyPage()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync(Phone, Password);

        var orders = await MyOrdersAsync(client);

        orders.Items.Should().BeEmpty();
        orders.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task MyOrders_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _fixture.Client.GetAsync("/api/orders/mine?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task MyOrders_WithInvalidPaging_ReturnsBadRequest(int page, int pageSize)
    {
        var client = await _fixture.CreateAuthenticatedClientAsync(Phone, Password);

        var response = await client.GetAsync($"/api/orders/mine?page={page}&pageSize={pageSize}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task FinalisingAPool_CreatesAPendingOrderAtTheVolumeDiscountedPrice()
    {
        var (merchant, productId) = await FinalisedPoolAsync();

        var orders = await MyOrdersAsync(merchant);

        var order = orders.Items.Should().ContainSingle().Subject;
        order.OrderStatus.Should().Be(OrderStatus.Pending);
        order.PaymentId.Should().BeNull();
        var line = order.OrderLines.Should().ContainSingle().Subject;
        line.ProductId.Should().Be(productId);
        line.Quantity.Should().Be(150);
        line.UnitPrice.Should().Be(53.99m, "150 units reaches the 100+ tier, which is 10% off 59.99");
        order.TotalAmount.Should().Be(53.99m * 150);
    }

    [Fact]
    public async Task PayingAnOrder_MarksItPaidAndRecordsThePayment()
    {
        var (merchant, _) = await FinalisedPoolAsync();
        var order = (await MyOrdersAsync(merchant)).Items.Single();

        var pay = await merchant.PostAsJsonAsync($"/api/orders/{order.OrderId}/pay", new { cardToken = "tok_test_visa_4242" });

        pay.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await pay.Content.ReadFromJsonAsync<PaymentBody>(TestJson.Options);
        result!.OrderStatus.Should().Be(OrderStatus.Paid);
        result.TransactionId.Should().NotBeNull();

        var after = (await MyOrdersAsync(merchant)).Items.Single();
        after.OrderStatus.Should().Be(OrderStatus.Paid);
        after.PaymentId.Should().Be(result.TransactionId);
    }

    [Fact]
    public async Task PayingAnOrderTwice_IsRejectedAndDoesNotChargeAgain()
    {
        var (merchant, _) = await FinalisedPoolAsync();
        var order = (await MyOrdersAsync(merchant)).Items.Single();
        (await merchant.PostAsJsonAsync($"/api/orders/{order.OrderId}/pay", new { cardToken = "tok" })).EnsureSuccessStatusCode();
        var firstPaymentId = (await MyOrdersAsync(merchant)).Items.Single().PaymentId;

        var second = await merchant.PostAsJsonAsync($"/api/orders/{order.OrderId}/pay", new { cardToken = "tok" });

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await MyOrdersAsync(merchant)).Items.Single().PaymentId.Should().Be(firstPaymentId);
    }

    [Fact]
    public async Task CancellingAPaidOrder_RefundsItAndMarksItCancelled()
    {
        var (merchant, _) = await FinalisedPoolAsync();
        var order = (await MyOrdersAsync(merchant)).Items.Single();
        (await merchant.PostAsJsonAsync($"/api/orders/{order.OrderId}/pay", new { cardToken = "tok" })).EnsureSuccessStatusCode();

        var cancel = await merchant.PostAsync($"/api/orders/{order.OrderId}/cancel", null);

        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        var after = (await MyOrdersAsync(merchant)).Items.Single();
        after.OrderStatus.Should().Be(OrderStatus.Cancelled);
        after.RefundId.Should().NotBeNull();
    }

    [Fact]
    public async Task AnotherMerchant_CannotPayOrCancelSomeoneElsesOrder()
    {
        var (owner, _) = await FinalisedPoolAsync();
        var order = (await MyOrdersAsync(owner)).Items.Single();
        var intruder = await _fixture.CreateAuthenticatedClientAsync(OtherPhone, Password);

        var pay = await intruder.PostAsJsonAsync($"/api/orders/{order.OrderId}/pay", new { cardToken = "tok" });
        var cancel = await intruder.PostAsync($"/api/orders/{order.OrderId}/cancel", null);

        pay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        cancel.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await MyOrdersAsync(owner)).Items.Single().OrderStatus.Should().Be(OrderStatus.Pending);
        (await MyOrdersAsync(intruder)).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ShippingAndDelivering_NeedTheAdminRole()
    {
        var (merchant, _) = await FinalisedPoolAsync();
        var order = (await MyOrdersAsync(merchant)).Items.Single();

        var ship = await merchant.PostAsync($"/api/orders/{order.OrderId}/ship", null);
        var deliver = await merchant.PostAsync($"/api/orders/{order.OrderId}/deliver", null);

        ship.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        deliver.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record PaymentBody(OrderStatus OrderStatus, Guid? TransactionId);
}
