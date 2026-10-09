using Xunit;
using FluentAssertions;
using Domain.ValueObjects;

namespace Domain.Tests.ValueObjects;
public class OrderLineTests
{
    [Fact]
    public void
    Constructor_WhenProductIdIsTheDefaultValue_ShouldThrowArgumentException()
    {
        Action act = () =>  OrderLine.Create(default(ProductId), Quantity.Create(5), Money.Create(5m));
        act.Should().Throw<ArgumentException>();   
    }

    [Fact]
    public void
    Constructor_WhenQuantityIsTheDefaultValue_ShouldThrowArgumentException()
    {
        Action act = () =>  OrderLine.Create(ProductId.New(), default(Quantity), Money.Create(5m));
        act.Should().Throw<ArgumentException>();   
    }
    
    [Fact]
    public void
    Constructor_WhenEqualOrderLinesCompared_ShouldBeEqual()
    {
        Guid id = Guid.NewGuid();

        OrderLine o1 = OrderLine.Create(ProductId.Create(id), Quantity.Create(5), Money.Create(5m));
        OrderLine o2 = OrderLine.Create(ProductId.Create(id), Quantity.Create(5), Money.Create(5m));
        o1.Should().Be(o2);
    }

}