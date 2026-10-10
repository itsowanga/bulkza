using Xunit;
using FluentAssertions;
using System.Reflection;
using Domain.ValueObjects;
using Domain.Enums;
using Domain.Tests.Builders;

namespace Domain.Tests;

public class CartTests()
{
    
    [Fact]
    public void
    Constructor_WhenCartIdIsTheDefault_ShouldThrowArgumentException()
    {
        Action act = () => new CartBuilder().WithCartId(default).Build();
        act.Should().Throw<ArgumentException>();
    }   

    [Fact]
    public void
    Constructor_WhenMerchantIdIsTheDefault_ShouldThrowArgumentException()
    {
        Action act = () => new CartBuilder().WithMerchantId(default).Build();
        act.Should().Throw<ArgumentException>();
    } 

    [Fact]
    public void
    Method_WhenProductIdIsTheDefault_ShouldThrowArgumentException()
    {
        var c = new CartBuilder().Build();
        Action act = () => c.AddItem(default, Quantity.Create(5));
        act.Should().Throw<ArgumentException>();
    } 

    [Fact]
    public void
    Method_WhenQuantityIsTheDefault_ShouldThrowArgumentException()
    {
        var c = new CartBuilder().Build();
        Action act = () => c.AddItem(ProductId.New(), default);
        act.Should().Throw<ArgumentException>();
    } 

    [Fact]
    public void
    Method_WhenAddingExistingCartItem_ShouldUpdateTheQuantity()
    {
        ProductId productId = ProductId.New();
        var c = new CartBuilder().Build();
        c.AddItem(productId, Quantity.Create(5));
        c.AddItem(productId, Quantity.Create(10));
        var q = c.CartItems.ToList<CartItem>();
        int index = q.FindIndex(i => i.ProductId == productId);
        q[index].Quantity.Value.Should().Be(15);
    }

    [Fact]
    public void
    Method_WhenAddingNewCartItem_ShouldBeAddedToCart()
    {
        ProductId productId = ProductId.New();
        var c = new CartBuilder().Build();
        c.AddItem(productId, Quantity.Create(5));
        c.CartItems.Should().Contain(CartItem.Create(productId, Quantity.Create(5)));
    }

    [Fact]
    public void
    Method_WhenRemovingNewCartItem_ShouldThrowInvalidOperationException()
    {
        ProductId productId = ProductId.New();
        var c = new CartBuilder().Build();
        Action act = () => c.RemoveItem(ProductId.New());
        act.Should().Throw<InvalidOperationException>();
    }

   [Fact]
    public void
    Method_WhenRemovingExistingCartItem_ShouldBeRemovedFromCart()
    {
        ProductId productId = ProductId.New();
        var c = new CartBuilder().Build();
        c.AddItem(productId, Quantity.Create(5));
        c.RemoveItem(productId);
        c.CartItems.Should().NotContain(CartItem.Create(productId, Quantity.Create(5)));

    }

   [Fact]
    public void
    Method_WhenUpdatingQuantityOfNonExistingCartItem_ShouldThrowInvalidOperationException()
    {
        ProductId productId = ProductId.New();
        Action act = () => new CartBuilder().Build().UpdateQuantity(productId, Quantity.Create(55));
        act.Should().Throw<InvalidOperationException>();
    }
}