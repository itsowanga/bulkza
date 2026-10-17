using Domain.Enums;

namespace Application.PaymentGateway;

public record ProcessPaymentResult(OrderStatus OrderStatus, Guid? TransactionId);