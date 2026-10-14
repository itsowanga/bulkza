using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.CreateOrder;

public record CreateOrderResult(OrderId OrderId, MerchantId MerchantId, IReadOnlyList<OrderLine> OrderLines, OrderStatus Status);