namespace Domain.Events;
public record PoolThresholdReached(
    EventId EventId,
    DateTimeOffset OccurredAt,
    PoolId PoolId,
    int CurrentQuantity,
    int ParticipantCount 
) : IDomainEvent;