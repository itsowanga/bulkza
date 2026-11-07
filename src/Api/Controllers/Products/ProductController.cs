using System.ComponentModel.DataAnnotations;
using Application.Products.GetProduct;
using Application.Products.SearchProduct.BrowseActiveProducts;
using Application.Products.SearchProduct.SearchProductByName;
using Application.Products.SearchProduct.SearchProductBySku;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Products;

[ApiController]
[Route("api/products")]
public class ProductsController (BrowseActiveProductsHandler browseProducts, 
                                SearchProductByNameHandler searchByName, 
                                SearchProductBySkuHandler searchBySku,
                                GetProductHandler getProduct
                                ) : ControllerBase
{
    private readonly BrowseActiveProductsHandler _browseProducts = browseProducts;
    private readonly SearchProductByNameHandler _searchName = searchByName;
    private readonly SearchProductBySkuHandler _searchSku = searchBySku;
    private readonly GetProductHandler _getProduct = getProduct;

    [Authorize]
    [HttpGet("browse")]
    public async Task<IActionResult> BrowseActiveProducts([FromQuery] int page, [FromQuery] int pageSize, [FromQuery] ProductCategory? category, CancellationToken ct)
    {
        var result = await _browseProducts.Handle(new(page, pageSize, category), ct);
        return Ok(result);

    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct)
    {
        var result = await _getProduct.Handle(new(ProductId.Create(id)), ct);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("search/name")]
    public async Task<IActionResult> SearchProductByName([FromQuery] string? name, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct = default)
    {
        var result = await _searchName.Handle(new(name, page, pageSize), ct);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("search/sku")]
    public async Task<IActionResult> SearchByProductSku([FromQuery, Required, MinLength(15)] string sku, CancellationToken ct = default)
    {
        var result = await _searchSku.Handle(new(sku), ct);
        return Ok(result);
    }

    
}