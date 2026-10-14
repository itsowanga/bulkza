using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.MarkAsPaid;

public record MarkAsPaidCommand(OrderId OrderId);