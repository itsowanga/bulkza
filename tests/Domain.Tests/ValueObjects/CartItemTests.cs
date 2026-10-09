using Xunit;
using FluentAssertions;
using Domain.ValueObjects;

namespace Domain.Tests.ValueObjects;

public class CartItemTests
{
    
    [Fact]
    public void
    Constructor_WhenProductIdIsTheDefault()
    {
        Action act = () => CartItem.Create(default(ProductId), Quantity.Create(5));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenQuantityIsTheDefault()
    {
        Action act = () => CartItem.Create(ProductId.New(), default(Quantity));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenEqualCartItemsCompared_ShouldBeEqual()
    {
        Guid id = Guid.NewGuid();
        CartItem c1 = CartItem.Create(ProductId.Create(id), Quantity.Create(5));
        CartItem c2 = CartItem.Create(ProductId.Create(id), Quantity.Create(5));
        c1.Should().Be(c2);
    }
}