namespace Domain.ValueObjects;
public readonly record struct EventId
{
    public Guid Value {get;}

    private EventId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("EventId cannot be empty");
        Value = value;
    }
    public static EventId New() => new(Guid.NewGuid());
    public static EventId Create(Guid value) => new(value);
    public override string ToString() => Value.ToString();
    
}
