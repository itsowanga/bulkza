using Domain.Enums;

namespace Application.PaymentGateway.ProcessPayment;

public record ProcessPaymentResult(OrderStatus OrderStatus, Guid? TransactionId);