using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.MarkAsDelivered;

public record MarkAsDeliveredResult(Guid OrderId, OrderStatus Status);