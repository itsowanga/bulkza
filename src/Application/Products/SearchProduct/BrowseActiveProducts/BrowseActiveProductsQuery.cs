using Domain.Enums;

namespace Application.Products.SearchProduct.BrowseActiveProducts;

public record BrowseActiveProductsQuery(int Page, int PageSize, ProductCategory? Category);