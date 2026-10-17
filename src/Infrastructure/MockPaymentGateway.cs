using Application.Interfaces;
using Application.ValueObjects;
using Domain.ValueObjects;

namespace Infrastructure;
public class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(Money amount, string cardToken, CancellationToken ct = default)
    {
        Task.Delay(1000, ct);
        return Task.FromResult(PaymentResult.Succeeded(Guid.NewGuid()));

    }

    public Task<RefundResult> RefundAsync(Guid? transactionId, Money amount, CancellationToken ct = default)
    {
        Task.Delay(1000, ct);
        return Task.FromResult(RefundResult.Succeeded(transactionId));
    }
}