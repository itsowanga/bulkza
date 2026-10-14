using Domain;
using Domain.ValueObjects;

namespace Application.Products.UpdateDescription;

public record UpdateDescriptionResult(ProductId ProductId, string? Description);