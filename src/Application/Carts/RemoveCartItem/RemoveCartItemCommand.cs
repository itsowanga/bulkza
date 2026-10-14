using Domain;
using Domain.ValueObjects;

namespace Application.Carts.RemoveCartItem;

public record RemoveCartItemCommand(CartId CartId, ProductId ProductId);