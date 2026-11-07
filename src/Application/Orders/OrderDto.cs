using Domain;
using Domain.Enums;

namespace Application.Orders;
public record OrderDto(Guid OrderId, Guid MerchantId, OrderStatus OrderStatus, decimal TotalAmount, DateTimeOffset CreatedAt
                        ,Guid? RefundId, Guid? PaymentId, IReadOnlyList<OrderLineDto> OrderLines)
{
    public static OrderDto From(Order order) => new(
        order.OrderId.Value,
        order.MerchantId.Value,
        order.Status,
        order.TotalAmount.Amount,
        order.CreatedAt,
        order.RefundId,
        order.PaymentId,
        order.OrderLines
             .Select(line => new OrderLineDto(line.ProductId.Value, line.Quantity.Value, line.UnitPrice.Amount))
             .ToList());
}
