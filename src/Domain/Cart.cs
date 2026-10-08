using Domain.ValueObjects;

namespace Domain;

public class Cart
{
    public CartId CartId {get; private set;}
    public MerchantId MerchantId {get; private set;}
    private List<CartItem> _cartItems;

    public Cart(CartId cartId, MerchantId merchantId)
    {
        if (cartId == default)
        {
            throw new ArgumentException($"{nameof(cartId)} cannot be the default value.");

        }

        if (merchantId == default)
        {
            throw new ArgumentException($"{nameof(merchantId)} cannot be the default value.");

        }

        CartId = cartId;
        MerchantId = merchantId;
        _cartItems = new List<CartItem>();
    }

    public IReadOnlyList<CartItem> CartItems => _cartItems;


    public void AddItem(ProductId productId, Quantity quantity)
    {
        if (productId == default)
        {
            throw new ArgumentException($"{nameof(productId)} cannot be the default value.");
        }

        if (quantity == default)
        {
            throw new ArgumentException($"{nameof(quantity)} cannot be the default value.");
        }

        int index = _cartItems.FindIndex(i => i.ProductId == productId);
        if (index >= 0)
        {
            CartItem foundItem = _cartItems[index];
            int newQuantity = foundItem.Quantity.Value + quantity.Value;
            _cartItems[index] = CartItem.Create(productId, Quantity.Create(newQuantity));
        }
        else
        {
            _cartItems.Add(CartItem.Create(productId, quantity));
        }

    }

    public void RemoveItem(ProductId productId)
    {

        if (productId == default)
        {
            throw new ArgumentException($"{nameof(productId)} cannot be the default value.");

        }
        
        int index = _cartItems.FindIndex(i => i.ProductId == productId);

        if (index < 0)
        {
            throw new InvalidOperationException($"{nameof(productId)} doesn't exist.");

        }

        _cartItems.Remove(_cartItems[index]);
    }

   public void UpdateQuantity(ProductId productId, Quantity quantity)
    {

        if (productId == default)
        {
            throw new ArgumentException($"{nameof(productId)} cannot be the default value.");
        }

        if (quantity == default)
        {
            throw new ArgumentException($"{nameof(quantity)} cannot be the default value.");
        }
       
       
        int index = _cartItems.FindIndex(i => i.ProductId == productId);

        if (index < 0)
        {
            throw new InvalidOperationException($"{nameof(productId)} doesn't exist.");
        }

         _cartItems[index] = CartItem.Create(productId, quantity);

    }

    public void Clear()
    {
        _cartItems.Clear();
    }

    public int GetTotalQuantity()
    {
      return _cartItems.Sum(i => i.Quantity.Value);
    }

    public bool IsEmpty()
    {
       return _cartItems.Count == 0;
    }


}