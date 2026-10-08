using Domain.ValueObjects;

public readonly record struct OrderLine
{
    public ProductId ProductId {get;}
    public Quantity Quantity {get;}
    public Money UnitPrice {get;}

    private OrderLine(ProductId productId, Quantity quantity, Money unitPrice)
    {
        if (productId == default)
        {
            throw new ArgumentException($"{nameof(productId)} cannot be the default value.");
        }
        if (quantity == default)
        {
            throw new ArgumentException($"{nameof(quantity)} cannot be the default value.");
        }
 
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static OrderLine Create(ProductId productId, Quantity quantity, Money unitPrice) => new(productId, quantity, unitPrice);
    public override string ToString() => $"{ProductId}: {Quantity} @ {UnitPrice}";
}