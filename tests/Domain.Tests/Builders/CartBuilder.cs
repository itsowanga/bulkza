using Domain.ValueObjects;

namespace Domain.Tests.Builders;

public class CartBuilder()
{
    private CartId _cartId = CartId.New();
    private MerchantId _merchantId = MerchantId.New();

    public CartBuilder WithCartId(CartId cartId)
    {
        _cartId = cartId;
        return this;
    }

    public CartBuilder WithMerchantId(MerchantId merchantId)
    {
        _merchantId = merchantId;
        return this;
    }

    public Cart Build() => new(_cartId, _merchantId);
}