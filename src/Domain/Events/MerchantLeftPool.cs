using Domain.ValueObjects;

namespace Domain.Events;
public record MerchantLeftPool(
    EventId EventId,
    DateTimeOffset OccurredAt,
    PoolId PoolId,
    MerchantId MerchantId,
    Quantity Quantity
) : IDomainEvent;