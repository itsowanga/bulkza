using Domain.ValueObjects;

namespace Domain.Events;
public record PoolExpired(
    EventId EventId,
    DateTimeOffset OccurredAt,
    PoolId PoolId
) : IDomainEvent;