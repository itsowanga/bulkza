using Domain.Enums;

namespace Application.Orders.CancelOrder;

public record CancelOrderResult(Guid OrderId, OrderStatus Status, Guid? RefundTransactionId);