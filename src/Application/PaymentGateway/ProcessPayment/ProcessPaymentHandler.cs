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
        Order order_ = await _order.GetOrderByIdAsync(cmd.OrderId, ct) ?? throw new KeyNotFoundException("Order is not found.");
        PaymentResult paymentResult = await _payment.ChargeAsync(order_.TotalAmount, cmd.cardToken, ct);
        if(paymentResult.Success)
        {
            order_.MarkAsPaid(paymentResult.TransactionId);
            _order.Update(order_);
            await _uow.SaveChangesAsync(ct);
        }
        
        return new(order_.Status, order_.PaymentId);
    }
}