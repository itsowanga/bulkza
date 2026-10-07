namespace Domain.ValueObjects;

public readonly record struct ProductId
{
    public Guid Value {get;}

    private ProductId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Product Id cannot be empty");
        }
            Value = value;
    }
    public static ProductId New() => new(Guid.NewGuid());
    public static ProductId Create(Guid value) => new(value);
    public override string ToString() => Value.ToString();
    
}