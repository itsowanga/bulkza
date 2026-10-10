using Domain.ValueObjects;

namespace Domain;
public class VolumePricingStrategy : IPricingStrategy
{
    private readonly IReadOnlyList<DiscountTier> _tiers;

    public VolumePricingStrategy(IEnumerable<DiscountTier> tiers)
    {
        ArgumentNullException.ThrowIfNull(tiers);
        _tiers = tiers.ToList().AsReadOnly();
    }

    public Money CalculateUnitPrice(Money basePrice, int quantity)
    {

        decimal highestDiscount = 0m;

        foreach (var discountTier in _tiers)
        {
            if (quantity >= discountTier.MinimumQuantity.Value && highestDiscount < discountTier.DiscountPercentage)
            {
                highestDiscount = discountTier.DiscountPercentage;   
            }
        }

        decimal newBasePrice = basePrice.Amount * ((100m - highestDiscount)/100);
        return Money.Create(newBasePrice);
    }

}