namespace Domain.ValueObjects;

public readonly record struct CartId
{
    public Guid Value {get;}

    private CartId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Cart Id cannot be empty");
        Value = value;
    }
    public static CartId New() => new(Guid.NewGuid());
    public static CartId Create(Guid value) => new(value);
    public override string ToString() => Value.ToString();
    
}