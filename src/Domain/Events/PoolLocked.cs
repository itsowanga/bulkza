using Domain.ValueObjects;

namespace Domain.Events;
public record PoolLocked(
    EventId EventId,
    DateTimeOffset OccurredAt,
    PoolId PoolId
) : IDomainEvent;