using Domain;
using Domain.ValueObjects;

namespace Application.Products.ActivateProduct;

public record ActivateProductCommand(ProductId ProductId);