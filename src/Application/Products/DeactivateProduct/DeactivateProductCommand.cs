using Domain;
using Domain.ValueObjects;
namespace Application.Products.DeactivateProduct;

public record DeactivateProductCommand(ProductId ProductId);