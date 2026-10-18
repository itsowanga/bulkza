using Application.Interfaces;
using Domain;

namespace Application.Products.SearchProduct.SearchProductByName;

public class SearchProductByNameHandler(IProductRepository product)
{
    private readonly IProductRepository _product = product;

    public async Task<PagedResult<ProductDto>> Handle(SearchProductByNameQuery query, CancellationToken ct)
    {
        var (products, totalCount) = await _product.GetByNameAsync(query.Name, query.Page, query.PageSize, ct);
        var dtos = products.Select(product => new ProductDto(
                            product.ProductId.Value,
                            product.Name,
                            product.SKU,
                            product.BasePrice.Amount
        )).ToList();

        return new (dtos, totalCount, query.Page, query.PageSize);

    }
}