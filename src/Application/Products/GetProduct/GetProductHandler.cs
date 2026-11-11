using Application.Interfaces;
using Application.Products.SearchProduct;
using Domain.ValueObjects;

namespace Application.Products.GetProduct;

public record GetProductQuery(ProductId ProductId);

public class GetProductHandler(IProductRepository product)
{
    private readonly IProductRepository _product = product;

    public async Task<ProductDto> Handle(GetProductQuery query, CancellationToken ct)
    {
        var product = await _product.GetByIdAsync(query.ProductId, ct) ?? throw new KeyNotFoundException("Product does not exist.");
        return ProductDto.From(product);
    }
}
