using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.MarkAsDelivered;

public record MarkAsDeliveredCommand(OrderId OrderId);