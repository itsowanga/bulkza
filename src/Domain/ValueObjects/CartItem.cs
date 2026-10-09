namespace Domain.ValueObjects;
public readonly record struct CartItem
{
    public ProductId ProductId {get;}
    public Quantity Quantity {get;}

    private CartItem(ProductId productId, Quantity quantity)
    {
        if (productId == default)
        {
            throw new ArgumentException($"{nameof(productId)} cannot be the default value.", nameof(productId));
        }

        if (quantity == default)
        {
            throw new ArgumentException($"{nameof(quantity)} cannot be the default value.", nameof(quantity));
        }

        ProductId = productId;
        Quantity = quantity;
    }

    public static CartItem Create(ProductId productId, Quantity quantity) => new(productId, quantity);
    public override string ToString() => $"{ProductId}: {Quantity}";
}