using Domain.ValueObjects;

namespace Application.Products.CreateProduct;

public record CreateProductResult(Guid ProductId, string Name, string Sku);