using Application.Interfaces;
using Application.ValueObjects;
using Domain.ValueObjects;

namespace Infrastructure;
public class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(Money amount, string cardToken, CancellationToken ct = default)
    {
        Task.Delay(1000, ct);
        PaymentResult result = PaymentResult.Succeeded(Guid.NewGuid());
        return Task.FromResult(result);

    }

    public Task<RefundResult> RefundAsync(string transactionId, Money amount, CancellationToken ct = default)
    {
        Task.Delay(1000, ct);
        RefundResult result = RefundResult.Succeeded(Guid.NewGuid());
        return Task.FromResult(result);
    }
}