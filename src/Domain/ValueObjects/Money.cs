namespace Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount {get;}
    
    private Money(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount must be greater or equal to zero.");
        }

        if (Math.Round(amount, 2) != amount)
        {
            throw new ArgumentException("Amount should not be more than two decimal places.");

        }

        Amount = amount;
    }

    public static Money Create(decimal amount) => new(amount);
    public override string ToString() => Amount.ToString("F2");


}