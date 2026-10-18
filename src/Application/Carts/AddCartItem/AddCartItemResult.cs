using Domain.ValueObjects;

namespace Application.Carts.AddCartItem;

public record AddCartItemResult(Guid CartId, Guid MerchantId, IReadOnlyList<CartItemDto> CartItems, int TotalQuantity);