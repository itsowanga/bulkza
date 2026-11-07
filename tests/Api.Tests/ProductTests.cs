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
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>(TestJson.Options);
        result!.PageSize.Should().Be(10); 
        result.Items[0].ProductId.Should().Be(productId.Value);
    }

    [Fact]
    public async Task BrowseActiveProducts_WithAValidCategory_ReturnsPagedResult()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        ProductId firstproductId = ProductId.Create(Guid.NewGuid());
        ProductId secondproductId = ProductId.Create(Guid.NewGuid());
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(firstproductId, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                        , new Product(secondproductId, "sku-12345678911",
                                                                    "Sunflower Oil 1L", null, ProductCategory.CookingOil,
                                                                    UnitOfMeasure.Litre, Money.Create(79.99m), true)
                                                                    )
                                                                    ;

        var response = await client.GetAsync($"/api/products/browse?page=1&pageSize=10&category=0");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>(TestJson.Options);
        result!.PageSize.Should().Be(10); 
        result!.Page.Should().Be(1);
        result.Items.Count.Should().Be(1);
        result.Items[0].ProductId.Should().Be(firstproductId.Value);
    }

    [Fact]
    public async Task BrowseActiveProducts_WithInvalidCategory_ReturnsBadRequest()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        ProductId productId = ProductId.Create(Guid.NewGuid());
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(productId, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await client.GetAsync($"/api/products/browse?page=1&pageSize=10&category=10");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BrowseActiveProducts_WithInvalidPagination_ReturnsBadRequest()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        ProductId productId = ProductId.Create(Guid.NewGuid());
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(productId, "sku-12345678910",
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await client.GetAsync($"/api/products/browse?page=0&pageSize=100&category=0");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProducts_WithValidSku_ReturnsProduct()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        string sku = "sku-12345678910";
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(ProductId.Create(Guid.NewGuid()), sku,
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await client.GetAsync($"/api/products/search/sku?sku=sku-12345678910");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(TestJson.Options);
        result!.Sku.Should().Be(sku);
    }

    [Fact]
    public async Task SearchProducts_WithInvalidSku_ReturnsBadRequest()
    {
        var client = await _fixture.CreateAuthenticatedClientAsync("0859954789", "$tr0ngP@ssw0rd");
        string sku = "sku-12345678910";
        await TestDataSeeder.SeedProductsAsync(_fixture, new Product(ProductId.Create(Guid.NewGuid()), sku,
                                                                    "White Star 1kg", null, ProductCategory.MaizeMeal, 
                                                                    UnitOfMeasure.Kilogram, Money.Create(59.99m), true)
                                                                    );

        var response = await client.GetAsync($"/api/products/search/sku?sku=sku-1234678910");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}