using System.Net.Http.Json;
using Application;
using Application.Products.SearchProduct;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

[Collection("api")]
public class ProductTests
{
    private readonly ApiSmokeTestFixture _fixture;

    public ProductTests(ApiSmokeTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task BrowseActiveProducts_WithValidFields_ReturnsPagedResult()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        ProductId productId = ProductId.Create(Guid.NewGuid());
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(productId, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true));

        var response = await client.GetAsync($"/api/products/browse?page=1&pageSize=10&category={null}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        result!.PageSize.Should().Be(10); 
        result.Items[0].ProductId.Should().Be(productId.Value);
    }
}