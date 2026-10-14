using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.CancelOrder;

public record CancelOrderResult(OrderId OrderId, MerchantId MerchantId, IReadOnlyList<OrderLine> OrderLines, OrderStatus Status);