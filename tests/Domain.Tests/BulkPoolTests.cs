using Xunit;
using FluentAssertions;
using System.Reflection;
using Domain.ValueObjects;
using Domain.Enums;
using Domain.Tests.Builders;

namespace Domain.Tests;

public class BulkPoolTests
{
    [Fact]
    public void
    Constructor_WhenPoolIdIsTheDefault_ShouldThrowArgumentException()
    {
        Action act = () => new BulkPoolBuilder().WithPoolId(default).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenProductIdIsTheDefault_ShouldThrowArgumentException()
    {
        Action act = () => new BulkPoolBuilder().WithProductId(default).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenMinimumQuantityIsTheDefault_ShouldThrowArgumentException()
    {
        Action act = () => new BulkPoolBuilder().WithMinimumQuantity(default).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenDeadlineIsNotInTheFuture_ShouldThrowArgumentException()
    {
        Action act = () => new BulkPoolBuilder().WithDeadline(DateTimeOffset.UtcNow - TimeSpan.FromDays(10)).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenMinimumMerchantsIsLessThanOne_ShouldThrowArgumentException()
    {
        Action act = () => new BulkPoolBuilder().WithMinimumMerchants(0).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Method_WhenParticipantJoinsAndThresholdTriggered_ShouldUpdateStatus()
    {
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(MerchantId.New(), Quantity.Create(5));
        b.Status.Should().Be(BulkPoolStatus.ThresholdMet);
    }  

    [Fact]
    public void
    Method_WhenParticipantJoinsAndStatusIsNotOpen_ShouldThrowInvalidOperationException()
    {
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(MerchantId.New(), Quantity.Create(5));
        Action act = () => b.Join(MerchantId.New(), Quantity.Create(5));
        act.Should().Throw<InvalidOperationException>();
        
    }  

    [Fact]
    public void
    Method_WhenParticipantJoinsAndDeadlineHasPassed_ShouldThrowInvalidOperationException()
    {
        var b = new BulkPoolBuilder().WithMinimumMerchants(2).WithMinimumQuantity(Quantity.Create(1)).Build();
        var property = typeof(BulkPool).GetProperty(nameof(BulkPool.Deadline))!;
        property.SetValue(b, DateTimeOffset.UtcNow.AddDays(-1));
        Action act = () => b.Join(MerchantId.New(), Quantity.Create(5));
        act.Should().Throw<InvalidOperationException>();
        
    }  
}