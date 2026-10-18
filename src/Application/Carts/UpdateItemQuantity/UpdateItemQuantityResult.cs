using Domain.ValueObjects;

namespace Application.Carts.UpdateItemQuantity;

public record UpdateItemQuantityResult(Guid CartId, Guid MerchantId, IReadOnlyList<CartItemDto> CartItems, int TotalQuantity);