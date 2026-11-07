using System.Text.Json;
using Domain.Events;
using Domain.ValueObjects;
using Infrastructure.Converters;

namespace Integration.Tests;

public class OutboxEventSerializationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new ValueObjectJsonConverterFactory() }
    };

    // Same shape the outbox stores: the event is serialised with default options.
    private static T RoundTrip<T>(T evt) where T : IDomainEvent
    {
        var json = JsonSerializer.Serialize(evt, evt.GetType());
        return (T)JsonSerializer.Deserialize(json, evt.GetType(), Options)!;
    }

    [Fact]
    public void MerchantJoinedPool_KeepsAllIds()
    {
        var evt = new MerchantJoinedPool(EventId.New(), DateTimeOffset.UtcNow, PoolId.New(), MerchantId.New(), Quantity.Create(5));

        var back = RoundTrip(evt);

        Assert.Equal(evt.EventId, back.EventId);
        Assert.Equal(evt.PoolId, back.PoolId);
        Assert.Equal(evt.MerchantId, back.MerchantId);
        Assert.Equal(evt.Quantity, back.Quantity);
        Assert.Equal(evt.OccurredAt, back.OccurredAt);
    }

    [Fact]
    public void MerchantLeftPool_KeepsAllIds()
    {
        var evt = new MerchantLeftPool(EventId.New(), DateTimeOffset.UtcNow, PoolId.New(), MerchantId.New(), Quantity.Create(3));

        Assert.Equal(evt, RoundTrip(evt));
    }

    [Fact]
    public void PoolThresholdReached_KeepsAllValues()
    {
        var evt = new PoolThresholdReached(EventId.New(), DateTimeOffset.UtcNow, PoolId.New(), 150, 4);

        Assert.Equal(evt, RoundTrip(evt));
    }

    [Fact]
    public void PoolLocked_KeepsAllIds() => AssertPoolEvent(new PoolLocked(EventId.New(), DateTimeOffset.UtcNow, PoolId.New()));

    [Fact]
    public void PoolFinalized_KeepsAllIds() => AssertPoolEvent(new PoolFinalized(EventId.New(), DateTimeOffset.UtcNow, PoolId.New()));

    [Fact]
    public void PoolExpired_KeepsAllIds() => AssertPoolEvent(new PoolExpired(EventId.New(), DateTimeOffset.UtcNow, PoolId.New()));

    [Fact]
    public void PoolCancelled_KeepsAllIds() => AssertPoolEvent(new PoolCancelled(EventId.New(), DateTimeOffset.UtcNow, PoolId.New()));

    [Fact]
    public void EmptyGuid_IsRejectedByTheValueObject()
    {
        var json = """{"Value":"00000000-0000-0000-0000-000000000000"}""";

        Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize<PoolId>(json, Options));
    }

    private static void AssertPoolEvent<T>(T evt) where T : IDomainEvent => Assert.Equal(evt, RoundTrip(evt));
}
