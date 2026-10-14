using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Products.UpdateBasePrice;

public record UpdateBasePriceCommand(ProductId ProductId, Money BasePrice);