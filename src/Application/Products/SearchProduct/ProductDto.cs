using Domain.ValueObjects;

namespace Application.Products.SearchProduct;

public record ProductDto(Guid ProductId, string Name, string Sku, decimal BasePrice);