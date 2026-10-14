using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.MarkAsShipped;

public record MarkAsShippedCommand(OrderId OrderId);