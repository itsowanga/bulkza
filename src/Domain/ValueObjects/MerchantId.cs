namespace Domain.ValueObjects;
public readonly record struct MerchantId
{
    public Guid Value {get;}

    private MerchantId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{nameof(value)} cannot be empty", nameof(value));
        }
            Value = value;
    }
    public static MerchantId New() => new(Guid.NewGuid());
    public static MerchantId Create(Guid value) => new(value);
    public override string ToString() => Value.ToString();
    
}
