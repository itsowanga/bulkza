namespace Application.Orders;
public record OrderLineDto(Guid ProductId, int Quantity, decimal UnitPrice);