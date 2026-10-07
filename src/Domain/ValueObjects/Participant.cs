namespace Domain.ValueObjects;
public readonly record struct Participant
{
    public MerchantId MerchantId {get;}
    public Quantity Quantity {get;}

    private Participant(MerchantId merchantId, Quantity quantity)
    {
        MerchantId = merchantId;
        Quantity = quantity;
    }

    public static Participant Create(MerchantId merchantId, Quantity quantity) => new(merchantId, quantity);
    public override string ToString() => $"{MerchantId}: {Quantity}";
}