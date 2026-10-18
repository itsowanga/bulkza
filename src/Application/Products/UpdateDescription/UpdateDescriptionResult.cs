using Domain;
using Domain.ValueObjects;

namespace Application.Products.UpdateDescription;

public record UpdateDescriptionResult(Guid ProductId, string? Description);