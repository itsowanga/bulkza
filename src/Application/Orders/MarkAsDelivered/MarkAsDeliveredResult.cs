using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.MarkAsDelivered;

public record MarkAsDeliveredResult(OrderId OrderId, MerchantId MerchantId, IReadOnlyList<OrderLine> OrderLines, OrderStatus Status);