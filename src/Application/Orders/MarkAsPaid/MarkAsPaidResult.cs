using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.MarkAsPaid;

public record MarkAsPaidResult(OrderId OrderId, MerchantId MerchantId, IReadOnlyList<OrderLine> OrderLines, OrderStatus Status);