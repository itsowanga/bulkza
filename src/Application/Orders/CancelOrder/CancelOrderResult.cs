using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.CancelOrder;

public record CancelOrderResult(Guid OrderId, OrderStatus Status, Guid RefundTransactionId);