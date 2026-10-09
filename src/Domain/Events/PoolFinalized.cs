using Domain.ValueObjects;

namespace Domain.Events;
public record PoolFinalized(
    EventId EventId,
    DateTimeOffset OccurredAt,
    PoolId PoolId
) : IDomainEvent;