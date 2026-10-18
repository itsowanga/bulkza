using Domain.ValueObjects;

namespace Application.Products.ActivateProduct;

public record ActivateProductResult(Guid ProductId, bool IsActive);