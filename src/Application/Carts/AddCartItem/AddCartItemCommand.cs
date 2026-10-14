using Domain.ValueObjects;

namespace Application.Carts.AddCartItem;

public record AddCartItemCommand(CartId CartId, ProductId ProductId, Quantity Quantity);