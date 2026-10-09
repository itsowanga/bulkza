using Xunit;
using FluentAssertions;
using Domain.ValueObjects;

namespace Domain.Tests.ValueObjects;
public class MoneyTests
{
    [Fact]
    public void
    Constructor_WhenAmountIsLessThanZero_ShouldThrowArgumentException()
    {
        Action act = () => Money.Create(-1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenAmountHasMoreThanTwoDecimals_ShouldThrowArgumentException()
    {
        Action act = () => Money.Create(5.5558m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenEqualAmountsAreCompared_ShouldBeEqual()
    {
        Money m1 = Money.Create(5m);
        Money m2 = Money.Create(5m);
        m1.Should().Be(m2);
    }

}