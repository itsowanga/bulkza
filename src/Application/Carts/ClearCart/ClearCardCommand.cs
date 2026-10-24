using Domain.ValueObjects;

namespace Application.Carts.ClearCart;

public record ClearCartCommand(MerchantId MerchantId);