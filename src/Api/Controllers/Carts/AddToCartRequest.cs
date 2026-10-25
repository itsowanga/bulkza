using Domain.ValueObjects;

namespace Api.Controllers.Carts;

public record AddToCartRequest(Guid ProductId, int Quantity);