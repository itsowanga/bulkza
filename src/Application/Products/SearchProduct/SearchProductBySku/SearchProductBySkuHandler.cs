using System.Runtime.InteropServices;
using Application.Interfaces;
using Domain;

namespace Application.Products.SearchProduct.SearchProductBySku;

public class SearchProductBySkuHandler(IProductRepository product)
{
    private readonly IProductRepository _product = product;

    public async Task<ProductDto> Handle(SearchProductBySkuQuery query, CancellationToken ct)
    {
        var result = await _product.GetBySkuAsync(query.Sku.Trim(), ct);
        if (result == default) throw new KeyNotFoundException("Product with the entered SKU is not found.");
        return ProductDto.From(result);
    }
}