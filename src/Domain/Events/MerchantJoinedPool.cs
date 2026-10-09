using Domain.ValueObjects;

namespace Domain.Events;
public record MerchantJoinedPool(
    EventId EventId,
    DateTimeOffset OccurredAt,
    PoolId PoolId,
    MerchantId MerchantId,
    Quantity Quantity
) : IDomainEvent;