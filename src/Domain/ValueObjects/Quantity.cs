namespace Domain.ValueObjects;
public readonly record struct Quantity
{
    public int Value {get;}

    private Quantity(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");

        }

        Value = value;
    }

    public static Quantity Create(int value) => new(value);
    public override string ToString() => Value.ToString();
}