using Domain.ValueObjects;

namespace Application.Orders.CreateOrder;

public record CreateOrderCommand(OrderId OrderId, MerchantId MerchantId, IReadOnlyList<OrderLine> OrderLines);