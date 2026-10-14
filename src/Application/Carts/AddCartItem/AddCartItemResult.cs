using Domain.ValueObjects;

namespace Application.Carts.AddCartItem;

public record AddCartItemResult(CartId CartId, MerchantId MerchantId, IReadOnlyList<CartItem> CartItems, int TotalQuantity);