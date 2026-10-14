using Domain.ValueObjects;

namespace Application.Carts.UpdateItemQuantity;

public record UpdateItemQuantityCommand(CartId CartId, ProductId ProductId, Quantity Quantity);