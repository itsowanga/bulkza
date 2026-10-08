namespace Domain;
public interface IDomainEvent
{
    public DateTimeOffset OccuredAt {get; private set;}
}