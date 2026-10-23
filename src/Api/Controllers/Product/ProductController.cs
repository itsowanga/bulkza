using Application.BulkPools.SearchPool.SearchOpenByProductId;
using Application.Products.CreateProduct;
using Application.Products.SearchProduct.BrowseActiveProducts;
using Application.Products.SearchProduct.SearchProductByName;
using Application.Products.SearchProduct.SearchProductBySku;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Product;

[ApiController]
[Route("api/products")]
public class ProductController (BrowseActiveProductsHandler browseProducts, 
                                SearchProductByNameHandler searchByName, 
                                SearchProductBySkuHandler searchBySku,
                                CreateProductHandler create
                                ) : ControllerBase
{
    private readonly BrowseActiveProductsHandler _browseProducts = browseProducts;
    private readonly SearchProductByNameHandler _searchName = searchByName;
    private readonly SearchProductBySkuHandler _searchSku = searchBySku;
    private readonly CreateProductHandler _create = create;


    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        CreateProductCommand command = new(request.Name, request.Description, request.Category,
                                    request.UnitOfMeasure, Money.Create(request.BasePrice));
        var result = await _create.Handle(command, ct);
        return Ok(result);
    }
    [Authorize]
    [HttpGet("browse")]
    public async Task<IActionResult> BrowseActiveProducts([FromBody] BrowseActiveProductsQuery query, CancellationToken ct)
    {
        var result = await _browseProducts.Handle(query, ct);
        return Ok(result);

    }

    [Authorize]
    [HttpGet("name")]
    public async Task<IActionResult> SearchProductByName([FromBody] SearchProductByNameQuery query, CancellationToken ct)
    {
        var result = await _searchName.Handle(query, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("sku")]
    public async Task<IActionResult> SearchByProductSku([FromBody] SearchProductBySkuQuery query, CancellationToken ct)
    {
        var result = await _searchSku.Handle(query, ct);
        return Ok(result);
    }

    
}