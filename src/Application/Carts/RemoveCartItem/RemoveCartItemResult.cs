using Domain;
using Domain.ValueObjects;

namespace Application.Carts.RemoveCartItem;

public record RemoveCartItemResult(CartId CartId, MerchantId MerchantId, IReadOnlyList<CartItem> CartItems, int TotalQuantity);