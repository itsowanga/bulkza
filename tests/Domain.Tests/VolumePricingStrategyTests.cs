using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests;

public class VolumePricingStrategyTests
{
    private static VolumePricingStrategy CreateStrategy() =>
        new(new[]
        {
            DiscountTier.Create(Quantity.Create(10), 5m),
            DiscountTier.Create(Quantity.Create(50), 10m),
            DiscountTier.Create(Quantity.Create(100), 15m),
        });

    [Fact]
    public void CalculateUnitPrice_NoTierMatched_ReturnsBasePrice()
    {
        var strategy = CreateStrategy();

        var result = strategy.CalculateUnitPrice(Money.Create(100m), 5);

        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void CalculateUnitPrice_QuantityExactlyAtTier_AppliesThatTier()
    {
        var strategy = CreateStrategy();

        var result = strategy.CalculateUnitPrice(Money.Create(100m), 10);

        result.Amount.Should().Be(95m);
    }

    [Fact]
    public void CalculateUnitPrice_QuantityBetweenTiers_AppliesLowerTier()
    {
        var strategy = CreateStrategy();

        var result = strategy.CalculateUnitPrice(Money.Create(100m), 49);

        result.Amount.Should().Be(95m);
    }

    [Fact]
    public void CalculateUnitPrice_QuantityAtHighestTier_AppliesHighestDiscount()
    {
        var strategy = CreateStrategy();

        var result = strategy.CalculateUnitPrice(Money.Create(100m), 100);

        result.Amount.Should().Be(85m);
    }

    [Fact]
    public void CalculateUnitPrice_QuantityAboveHighestTier_StillAppliesHighestDiscount()
    {
        var strategy = CreateStrategy();

        var result = strategy.CalculateUnitPrice(Money.Create(100m), 5000);

        result.Amount.Should().Be(85m);
    }

    [Fact]
    public void CalculateUnitPrice_ZeroQuantity_ReturnsBasePrice()
    {
        var strategy = CreateStrategy();

        var result = strategy.CalculateUnitPrice(Money.Create(100m), 0);

        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void CalculateUnitPrice_NegativeQuantity_ReturnsBasePrice()
    {
        var strategy = CreateStrategy();

        var result = strategy.CalculateUnitPrice(Money.Create(100m), -5);

        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void CalculateUnitPrice_NoTiersConfigured_ReturnsBasePrice()
    {
        var strategy = new VolumePricingStrategy(Array.Empty<DiscountTier>());

        var result = strategy.CalculateUnitPrice(Money.Create(100m), 1000);

        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void Constructor_NullTiers_Throws()
    {
        Action act = () => new VolumePricingStrategy(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateUnitPrice_UnorderedTiers_StillPicksHighestApplicableDiscount()
    {
        var strategy = new VolumePricingStrategy(new[]
        {
            DiscountTier.Create(Quantity.Create(100), 15m),
            DiscountTier.Create(Quantity.Create(10), 5m),
            DiscountTier.Create(Quantity.Create(50), 10m),
        });

        var result = strategy.CalculateUnitPrice(Money.Create(100m), 60);

        result.Amount.Should().Be(90m);
    }
}