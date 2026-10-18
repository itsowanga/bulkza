using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.CreateOrder;

public record CreateOrderResult(Guid OrderId, IReadOnlyList<OrderLineDto> OrderLines, OrderStatus Status);