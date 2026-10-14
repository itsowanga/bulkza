using Domain;
using Domain.ValueObjects;

namespace Application.Products.UpdateDescription;

public record UpdateDescriptionCommand(ProductId ProductId, string? Description);