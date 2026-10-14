using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Products.UpdateBasePrice;

public record UpdateBasePriceResult(ProductId ProductId, ProductCategory Category);