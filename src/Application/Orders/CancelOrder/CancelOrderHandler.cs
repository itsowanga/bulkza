using Application.Interfaces;
using Application.ValueObjects;
using Domain;

namespace Application.Orders.CancelOrder;

public class CancelOrderHandler(IOrderRepository order, IPaymentGateway payment, IUnitOfWork uow)
{
    private readonly IOrderRepository _order = order;
    private readonly IUnitOfWork _uow = uow;
    private readonly IPaymentGateway _payment = payment;

    public async Task<CancelOrderResult> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        Order order_ = await _order.GetOrderByIdAsync(cmd.OrderId, ct) ?? throw new KeyNotFoundException("Order is not found.");
        if (cmd.MerchantId != order_.MerchantId) throw new UnauthorizedAccessException("Merchant is not authorized to cancel this order.");
        order_.Cancel(Guid.NewGuid());
        RefundResult result =  await _payment.RefundAsync(order_.RefundId, order_.TotalAmount, ct);
        if(result.Success)
        {
            _order.Update(order_);
            await _uow.SaveChangesAsync(ct);
        }
        
        return new(order_.OrderId.Value, order_.Status, order_.RefundId);
    }
}