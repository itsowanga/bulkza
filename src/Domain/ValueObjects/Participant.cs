using System.Data;

namespace Domain.ValueObjects;
public readonly record struct Participant
{
    public MerchantId MerchantId {get;}
    public Quantity Quantity {get;}

    private Participant(MerchantId merchantId, Quantity quantity)
    {
        if (merchantId == default) throw new ArgumentException($"{nameof(merchantId)} should not be the default value.", nameof(merchantId));
        if (quantity == default) throw new ArgumentException($"{nameof(quantity)} should not be the default value.", nameof(quantity));
        
        MerchantId = merchantId;
        Quantity = quantity;
    }
    public static Participant Create(MerchantId merchantId, Quantity quantity) => new(merchantId, quantity);
    public override string ToString() => $"{MerchantId}: {Quantity}";
}