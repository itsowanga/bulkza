using Domain.Enums;

namespace Application.Orders;
public record OrderDto(Guid OrderId, Guid MerchantId, OrderStatus OrderStatus, decimal TotalAmount, DateTimeOffset CreatedAt
                        ,Guid? RefundId, Guid? PaymentId, IReadOnlyList<OrderLineDto> OrderLines);