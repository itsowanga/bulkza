using Xunit;
using FluentAssertions;
using Domain.ValueObjects;

namespace Domain.Tests.ValueObjects;
public class QuantityTest
{
    [Fact]
    public void
    Constructor_WhenQuantityIsNotGreaterThanZero_ShouldThrowArgumentException()
    {
        Action act = () => Quantity.Create(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenEqualQuantitiesAreCompared_ShouldBeEqual()
    {
        Quantity q1 = Quantity.Create(5);
        Quantity q2 = Quantity.Create(5);
        q1.Should().Be(q2);
    }
}