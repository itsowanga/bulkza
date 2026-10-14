using Domain.ValueObjects;

namespace Application.Products.DeactivateProduct;

public record DeactivateProductResult(ProductId ProductId, bool IsActive);