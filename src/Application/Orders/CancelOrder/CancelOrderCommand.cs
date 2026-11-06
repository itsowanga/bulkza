using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Orders.CancelOrder;

public record CancelOrderCommand(OrderId OrderId, MerchantId MerchantId);