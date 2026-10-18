namespace Application.Carts.ClearCart;

public record ClearCartResult(Guid CartId, Guid MerchantId, IReadOnlyList<CartItemDto> CartItems, int TotalQuantity);