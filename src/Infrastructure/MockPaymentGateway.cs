using Application.Interfaces;
using Application.ValueObjects;
using Domain.ValueObjects;

namespace Infrastructure;
public class MockPaymentGateway : IPaymentGateway
{
    public async Task<PaymentResult> ChargeAsync(Money amount, string cardToken, CancellationToken ct = default)
    {
        await Task.Delay(1000, ct);
        var result = PaymentResult.Succeeded(Guid.NewGuid());
        return result;

    }

    public async Task<RefundResult> RefundAsync(Guid? transactionId, Money amount, CancellationToken ct = default)
    {
        await Task.Delay(1000, ct);
        var result = RefundResult.Succeeded(transactionId);
        return result;
    }
}