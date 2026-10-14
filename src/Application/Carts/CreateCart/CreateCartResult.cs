using Domain.ValueObjects;

namespace Application.Carts.CreateCart;

public record CreateCartResult(CartId CartId, MerchantId MerchantId, IReadOnlyList<CartItem> CartItems, int TotalQuantity);