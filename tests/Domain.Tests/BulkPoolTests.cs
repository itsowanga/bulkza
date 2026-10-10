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

    [Fact]
    public void
    Method_WhenParticipantJoinsAndIsAlreadyInThePool_ShouldThrowInvalidOperationException()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(2).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(12));
        Action act = () => b.Join(merchantId, Quantity.Create(5));
        act.Should().Throw<InvalidOperationException>();
        
    }   

    [Fact]
    public void
    Method_WhenParticipantJoinsThePool_ShouldBeAddedIntoThePool()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(2).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(12));
        b.Participants.Should().Contain(Participant.Create(merchantId, Quantity.Create(12)));
        
    }   

    [Fact]
    public void
    Method_WhenParticipantJoinsThePool_ShouldUpdateTheCurrentQuantity()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(2).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(12));
        b.Join(MerchantId.New(), Quantity.Create(13));
        b.CurrentQuantity.Should().Be(25);
    }   

    [Fact]
    public void
    Method_WhenThePoolGetsLocked_ShouldUpdateTheStatus()
    {
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(MerchantId.New(), Quantity.Create(5));
        b.Lock();
        b.Status.Should().Be(BulkPoolStatus.Locked);
        
    } 

    [Fact]
    public void
    Method_WhenParticipantLeavesTheStatusIsNotOpenOrThresholdNotMet_ShouldThrowInvalidOperationException()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(5));
        b.Lock();
        Action act = () => b.Leave(merchantId);
        act.Should().Throw<InvalidOperationException>();
        
    } 

    [Fact]
    public void
    Method_WhenParticipantLeavesAndDeadlineHasPassed_ShouldThrowInvalidOperationException()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(2).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(5));
        var property = typeof(BulkPool).GetProperty(nameof(BulkPool.Deadline))!;
        property.SetValue(b, DateTimeOffset.UtcNow.AddDays(-1));
        Action act = () => b.Leave(merchantId);
        act.Should().Throw<InvalidOperationException>();
        
    } 
     
    [Fact]
    public void
    Method_WhenParticipantLeavesAndIsNotInThePool_ShouldThrowInvalidOperationException()
    {
        var b = new BulkPoolBuilder().WithMinimumMerchants(2).WithMinimumQuantity(Quantity.Create(1)).Build();
        Action act = () => b.Leave(MerchantId.New());
        act.Should().Throw<InvalidOperationException>();
        
    } 

    [Fact]
    public void
    Method_WhenParticipantLeavesThePool_ShouldBeRemoved()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(2).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(5));
        b.Leave(merchantId);
        b.Participants.Should().NotContain(Participant.Create(merchantId, Quantity.Create(5)));

    }
    
    [Fact]
    public void
    Method_WhenParticipantLeavesAndThresholdNotMet_ShouldUpdateStatusToOpen()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(5));
        b.Leave(merchantId);
        b.Status.Should().Be(BulkPoolStatus.Open);

    }

    [Fact]
    public void
    Method_WhenPoolIsLockedAndThresholdIsNotMet_ShouldThrowInvalidOperationException()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        Action act = () => b.Lock();
        act.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void
    Method_WhenPoolIsLocked_ShouldUpdateStatusToLocked()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(5));
        b.Lock();
        b.Status.Should().Be(BulkPoolStatus.Locked);

    }

    [Fact]
    public void
    Method_WhenPoolIsFinalizedAndNotLocked_ShouldThrowInvalidOperationException()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        Action act = () => b.Finalise();
        act.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void
    Method_WhenPoolIsFinalized_ShouldUpdateStatusToFinalized()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(5));
        b.Lock();
        b.Finalise();
        b.Status.Should().Be(BulkPoolStatus.Finalized);

    }

    [Fact]
    public void
    Method_WhenPoolIsExpiredBeforeDeadline_ShouldThrowInvalidOperationException()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(1)
                                     .WithMinimumQuantity(Quantity.Create(1))
                                     .Build();
        Action act = () => b.Expire();
        act.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void
    Method_WhenPoolIsExpiredAndStatusIsNotOpenOrThresholdMet_ShouldThrowInvalidOperation()
    {
        MerchantId merchantId = MerchantId.New();
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Join(merchantId, Quantity.Create(5));
        b.Lock();
        Action act = () => b.Expire();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void
    Method_WhenPoolIsExpired_ShouldUpdateStatusToExpired()
    {
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        var property = typeof(BulkPool).GetProperty(nameof(BulkPool.Deadline))!;
        property.SetValue(b, DateTimeOffset.UtcNow.AddDays(-1));
        b.Expire();
        b.Status.Should().Be(BulkPoolStatus.Expired);

    }

    [Fact]
    public void
    Method_WhenPoolIsCancelledAndPoolIsNotOpenOrThresholdNotMet_ShouldThrowInvalidOperationException()
    {
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        var property = typeof(BulkPool).GetProperty(nameof(BulkPool.Deadline))!;
        property.SetValue(b, DateTimeOffset.UtcNow.AddDays(-1));
        b.Expire();
        Action act = () => b.Cancel();
        act.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void
    Method_WhenPoolIsCancelled_ShouldUpdateStatusToCancelled()
    {
        var b = new BulkPoolBuilder().WithMinimumMerchants(1).WithMinimumQuantity(Quantity.Create(1)).Build();
        b.Cancel();
        b.Status.Should().Be(BulkPoolStatus.Cancelled);

    }
}