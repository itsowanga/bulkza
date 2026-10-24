using Domain.ValueObjects;

namespace Application.Carts.AddCartItem;

public record AddCartItemCommand(MerchantId MerchantId, ProductId ProductId, Quantity Quantity);