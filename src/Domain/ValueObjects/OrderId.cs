namespace Domain.ValueObjects;

public readonly record struct OrderId
{
    public Guid Value {get;}

    private OrderId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Order Id cannot be empty");
        }
            Value = value;
    }
    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId Create(Guid value) => new(value);
    public override string ToString() => Value.ToString();
    
}