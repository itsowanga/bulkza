using Domain.ValueObjects;

namespace Application.Products.DeactivateProduct;

public record DeactivateProductResult(Guid ProductId, bool IsActive);