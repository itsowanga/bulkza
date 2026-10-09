namespace Domain.ValueObjects;
public readonly record struct DiscountTier
{
    public Quantity MinimumQuantity {get;}
    public decimal DiscountPercentage {get;}

    private DiscountTier(Quantity minimumQuantity, decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
        {
            throw new ArgumentException($"{nameof(discountPercentage)} should be between 0 and 100.");

        }

        if (minimumQuantity == default)
        {
            throw new ArgumentException($"{nameof(minimumQuantity)} cannot be the default value.");
        }

        MinimumQuantity = minimumQuantity;
        DiscountPercentage = discountPercentage;
    }

    public static DiscountTier Create(Quantity minimumQuantity, decimal discountPercentage) => new(minimumQuantity, discountPercentage);
    public override string ToString() => $"{MinimumQuantity}+ => {DiscountPercentage}%";
}