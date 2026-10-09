using Xunit;
using FluentAssertions;
using Domain.ValueObjects;

namespace Domain.Tests.ValueObjects;

public class DiscountTierTests
{
    
    [Fact]
    public void
    Constructor_WhenDiscountPercentageIsNotInRange_ShouldThrowArgumentException()
    {
        Action act = () => DiscountTier.Create(Quantity.Create(5), 105m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenQuantityIsTheDefaultValue_ShouldThrowArgumentException()
    {
        Action act = () => DiscountTier.Create(default(Quantity), 95m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenEqualDiscountTiersCompared_ShouldBeEqual()
    {
        DiscountTier d1 = DiscountTier.Create(Quantity.Create(5), 5m);
        DiscountTier d2 = DiscountTier.Create(Quantity.Create(5), 5m);
        d1.Should().Be(d2);
    }
}