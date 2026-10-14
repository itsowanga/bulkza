using Domain.ValueObjects;

namespace Application.Products.CreateProduct;

public record CreateProductResult(ProductId ProductId, string Name, string Sku);