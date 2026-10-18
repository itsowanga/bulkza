using Domain;
using Domain.ValueObjects;

namespace Application.Carts.RemoveCartItem;

public record RemoveCartItemResult(Guid CartId, Guid MerchantId, IReadOnlyList<CartItemDto> CartItems, int TotalQuantity);