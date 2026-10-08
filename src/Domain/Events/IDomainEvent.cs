using Domain.ValueObjects;

namespace Domain.Events;
public interface IDomainEvent
{
    public EventId EventId {get;}
    public DateTimeOffset OccurredAt {get;}
}