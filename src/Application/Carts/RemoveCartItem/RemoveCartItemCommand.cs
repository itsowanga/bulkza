using Domain;
using Domain.ValueObjects;

namespace Application.Carts.RemoveCartItem;

public record RemoveCartItemCommand(MerchantId MerchantId, ProductId ProductId);