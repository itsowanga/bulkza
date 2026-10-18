using Domain.ValueObjects;

namespace Application.Carts.CreateCart;

public record CreateCartResult(Guid CartId, Guid MerchantId, IReadOnlyList<CartItemDto> CartItems, int TotalQuantity);