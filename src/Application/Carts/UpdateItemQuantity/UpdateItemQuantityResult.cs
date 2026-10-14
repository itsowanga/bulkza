using Domain.ValueObjects;

namespace Application.Carts.UpdateItemQuantity;

public record UpdateItemQuantityResult(CartId CartId, MerchantId MerchantId, IReadOnlyList<CartItem> CartItems, int TotalQuantity);