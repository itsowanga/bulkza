using Domain;
using Domain.ValueObjects;

namespace Application.PaymentGateway;

public record ProcessPaymentCommand(OrderId OrderId, string cardToken);