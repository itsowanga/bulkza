using Domain.ValueObjects;

namespace Application.Carts.ClearCart;

public record ClearCartResult(CartId CartId, MerchantId MerchantId, IReadOnlyList<CartItem> CartItems, int TotalQuantity);