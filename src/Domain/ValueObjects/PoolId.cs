namespace Domain.ValueObjects;

public readonly record struct PoolId
{
    public Guid Value {get;}

    private PoolId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Pool Id cannot be empty");
        Value = value;
    }
    public static PoolId New() => new(Guid.NewGuid());
    public static PoolId Create(Guid value) => new(value);
    public override string ToString() => Value.ToString();
    
}