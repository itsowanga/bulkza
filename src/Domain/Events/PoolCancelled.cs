namespace Domain.Events;
public record PoolCancelled(
    EventId EventId,
    DateTimeOffset OccurredAt,
    PoolId PoolId
) : IDomainEvent;