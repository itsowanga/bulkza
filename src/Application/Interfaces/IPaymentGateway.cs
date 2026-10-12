using Application.ValueObjects;
using Domain.ValueObjects;

namespace Application.Interfaces;
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(Money amount, string cardToken, CancellationToken ct = default);
    Task<RefundResult> RefundAsync(string transactionId, Money amount, CancellationToken ct = default);
}