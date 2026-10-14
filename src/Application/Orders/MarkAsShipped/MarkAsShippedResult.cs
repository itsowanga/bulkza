using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.MarkAsShipped;

public record MarkAsShippedResult(OrderId OrderId, MerchantId MerchantId, IReadOnlyList<OrderLine> OrderLines, OrderStatus Status);