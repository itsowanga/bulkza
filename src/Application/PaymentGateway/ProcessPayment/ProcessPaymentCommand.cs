using Domain;
using Domain.ValueObjects;

namespace Application.PaymentGateway.ProcessPayment;

public record ProcessPaymentCommand(OrderId OrderId, string CardToken);