using Domain.ValueObjects;

namespace Application.Products.ActivateProduct;

public record ActivateProductResult(ProductId ProductId, bool IsActive);