using Application.Interfaces;
using Application.ValueObjects;
using Domain;

namespace Application.PaymentGateway.ProcessPayment;

public class ProcessPaymentHandler(IPaymentGateway payment, IOrderRepository order, IUnitOfWork uow)
{
    private readonly IPaymentGateway _payment = payment;
    private readonly IUnitOfWork _uow = uow;
    private readonly IOrderRepository _order = order;

    public async Task<ProcessPaymentResult> Handle(ProcessPaymentCommand cmd, CancellationToken ct)
    {
        Order order = await _order.GetOrderByIdAsync(cmd.OrderId, ct) ?? throw new KeyNotFoundException("Order is not found.");
        if (order.MerchantId != cmd.MerchantId) throw new UnauthorizedAccessException("Merchant is not allowed to pay this order.");
        if (order.Status != Domain.Enums.OrderStatus.Pending) throw new InvalidOperationException("Cannot pay for order not pending.");
        PaymentResult paymentResult = await _payment.ChargeAsync(order.TotalAmount, cmd.CardToken, ct);
        if(paymentResult.Success)
        {
            order.MarkAsPaid(paymentResult.TransactionId);
            _order.Update(order);
            await _uow.SaveChangesAsync(ct);
        }
        
        return new(order.Status, order.PaymentId);
    }
}