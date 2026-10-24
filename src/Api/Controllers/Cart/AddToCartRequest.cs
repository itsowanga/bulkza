using Domain.ValueObjects;

namespace Api.Controllers.Cart;

public record AddToCartRequest(Guid ProductId, int Quantity);