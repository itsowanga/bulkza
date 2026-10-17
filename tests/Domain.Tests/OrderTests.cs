using Xunit;
using FluentAssertions;
using Domain.Tests.Builders;
using Domain.ValueObjects;

namespace Domain.Tests;

public class OrderTests
{

    [Fact]
    public void
    Constructor_WhenOrderIdIsTheDefault_ShouldThrowArgumentException()
    {
        Action act = () => new OrderBuilder().WithOrderId(default).Build();
        act.Should().Throw<ArgumentException>();
    }   

    [Fact]
    public void
    Constructor_WhenMerchantIdIsTheDefault_ShouldThrowArgumentException()
    {
        Action act = () => new OrderBuilder().WithMerchantId(default).Build();
        act.Should().Throw<ArgumentException>();
    }   

    [Fact]
    public void
    Constructor_WhenOrderLinesAreNull_ShouldThrowArgumentException()
    {
        Action act = () => new OrderBuilder().WithOrderLines(null!).Build();
        act.Should().Throw<ArgumentException>();
    }  

    [Fact]
    public void
    Constructor_WhenOrderLinesAreEmpty_ShouldThrowArgumentException()
    {
        Action act = () => new OrderBuilder().WithOrderLines([]).Build();
        act.Should().Throw<ArgumentException>();
    }  

    [Fact]
    public void
    Constructor_WhenTotalAmountIsCalculated_ShouldBeCorrect()
    {
        var o = new OrderBuilder().Build();
        o.TotalAmount.Should().Be(Money.Create(275));
    }  


    [Fact]
    public void
    Constructor_WhenMarkAsPaidAndOrderIsNotPending_ShouldThrowInvalidOperationException()
    {
        var o = new OrderBuilder().Build();
        o.MarkAsPaid(Guid.NewGuid());
        Action act = () => o.MarkAsPaid(Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>();
    }  

    [Fact]
    public void
    Constructor_WhenMarkAsShippedAndOrderIsNotPaid_ShouldThrowInvalidOperationException()
    {
        var o = new OrderBuilder().Build();
        Action act = () => o.MarkAsShipped();
        act.Should().Throw<InvalidOperationException>();
    }  

    [Fact]
    public void
    Constructor_WhenMarkAsDeliveredAndOrderIsNotShipped_ShouldThrowInvalidOperationException()
    {
        var o = new OrderBuilder().Build();
        Action act = () => o.MarkAsDelivered();
        act.Should().Throw<InvalidOperationException>();
    }  

    [Fact]
    public void
    Method_WhenCancelledAndOrderNotPendingAndNotPaid_ShouldThrowInvalidOperationException()
    {
        var o = new OrderBuilder().Build();
        o.MarkAsPaid(Guid.NewGuid());
        o.MarkAsShipped();
        Action act = () => o.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }  

    [Fact]
    public void
    Method_WhenOrderIsPaid_ShouldUpdateTheTransactionId()
    {
        var o = new OrderBuilder().Build();
        o.MarkAsPaid(Guid.NewGuid());
        o.TransactionId.Should().NotBe(Guid.Empty);
    }
}