using Application.Interfaces;
using Domain.Enums;

namespace Application.Products.SearchProduct.BrowseActiveProducts;

public class BrowseActiveProductsHandler(IProductRepository product)
{
    private readonly IProductRepository _product = product;

    public async Task<PagedResult<ProductDto>> Handle(BrowseActiveProductsQuery query, CancellationToken ct)
    {
        if (query.Page < 1) throw new ArgumentException("Page number cannot be less than 1.");
        if (query.PageSize > 50) throw new ArgumentException("Maximum page size is 50.");
        if (query.Category is not null && Enum.IsDefined((ProductCategory)query.Category) != true) throw new ArgumentException("Category entered is invalid.");
        var (products, totalProducts) = await _product.GetActiveAsync(query.Page, query.PageSize, query.Category, ct);
        var dtos = products.Select(product => new ProductDto(product.ProductId.Value,
                                product.Name,
                                product.SKU,
                                product.BasePrice.Amount)).ToList();

        return new(dtos, totalProducts, query.Page, query.PageSize);
        
    }
}