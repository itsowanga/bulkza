using Application.Orders;
using Domain;
using Domain.Enums;

namespace Application.Admin;

/// <summary>An order as an admin sees it: the same as the merchant's own order, plus who placed it.</summary>
public record AdminOrderDto(Guid OrderId, Guid MerchantId, string MerchantName, OrderStatus OrderStatus, decimal TotalAmount,
                            DateTimeOffset CreatedAt, Guid? RefundId, Guid? PaymentId, IReadOnlyList<OrderLineDto> OrderLines)
{
    public static AdminOrderDto From(Order order, string merchantName)
    {
        var dto = OrderDto.From(order);
        return new(dto.OrderId, dto.MerchantId, merchantName, dto.OrderStatus, dto.TotalAmount,
                   dto.CreatedAt, dto.RefundId, dto.PaymentId, dto.OrderLines);
    }
}

/// <summary>How many pools and orders are in each state. Every state is present, with zero when empty.</summary>
public record AdminSummaryDto(IReadOnlyDictionary<BulkPoolStatus, int> Pools, IReadOnlyDictionary<OrderStatus, int> Orders);
