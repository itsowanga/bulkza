using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.MarkAsShipped;

public record MarkAsShippedResult(Guid OrderId, OrderStatus Status);