using Application.Interfaces;

namespace Application.Products.SearchProduct.BrowseActiveProducts;

public class BrowseActiveProductsHandler(IProductRepository product)
{
    private readonly IProductRepository _product = product;

    public async Task<PagedResult<ProductDto>> Handle(BrowseActiveProductsQuery query, CancellationToken ct)
    {
        var (products, totalProducts) = await _product.GetActiveAsync(query.Page, query.PageSize, query.Category, ct);
        var dtos = products.Select(product => new ProductDto(product.ProductId.Value,
                                product.Name,
                                product.SKU,
                                product.BasePrice.Amount)).ToList();

        return new(dtos, totalProducts, query.Page, query.PageSize);
        
    }
}