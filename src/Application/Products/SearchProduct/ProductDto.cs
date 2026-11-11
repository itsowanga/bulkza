using Domain;
using Domain.Enums;

namespace Application.Products.SearchProduct;

public record DiscountTierDto(int MinimumQuantity, decimal DiscountPercentage);

public record ProductDto(Guid ProductId, string Name, string Sku, decimal BasePrice,
                         ProductCategory Category, UnitOfMeasure UnitOfMeasure,
                         IReadOnlyList<DiscountTierDto> DiscountTiers)
{
    public static ProductDto From(Product product) => new(
        product.ProductId.Value,
        product.Name,
        product.SKU,
        product.BasePrice.Amount,
        product.Category,
        product.UnitOfMeasure,
        product.DiscountTiers
               .OrderBy(tier => tier.MinimumQuantity.Value)
               .Select(tier => new DiscountTierDto(tier.MinimumQuantity.Value, tier.DiscountPercentage))
               .ToList());
}
