using Domain.ValueObjects;

namespace Application.Carts.UpdateItemQuantity;

public record UpdateItemQuantityCommand(MerchantId MerchantId, ProductId ProductId, Quantity Quantity);