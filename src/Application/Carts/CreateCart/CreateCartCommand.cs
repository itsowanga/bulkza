using Domain.ValueObjects;

namespace Application.Carts.CreateCart;

public record CreateCartCommand(CartId CartId, MerchantId MerchantId);