using Application.Interfaces;
using Application.ValueObjects;
using Domain;
using Domain.Enums;

namespace Application.Orders.CancelOrder;

public class CancelOrderHandler(IOrderRepository order, IPaymentGateway payment, IUnitOfWork uow)
{
    private readonly IOrderRepository _order = order;
    private readonly IUnitOfWork _uow = uow;
    private readonly IPaymentGateway _payment = payment;

    public async Task<CancelOrderResult> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        Order order = await _order.GetOrderByIdAsync(cmd.OrderId, ct) ?? throw new KeyNotFoundException("Order is not found.");
        if (cmd.MerchantId != order.MerchantId) throw new UnauthorizedAccessException("Merchant is not authorized to cancel this order.");

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Paid)
            throw new InvalidOperationException("Order cannot be cancelled.");

        if (order.Status == OrderStatus.Paid)
        {
            RefundResult refund = await _payment.RefundAsync(order.PaymentId, order.TotalAmount, ct);
            if (!refund.Success) throw new InvalidOperationException($"Refund failed: {refund.ErrorMessage}");
        }

        order.Cancel(Guid.NewGuid());
        _order.Update(order);
        await _uow.SaveChangesAsync(ct);

        return new(order.OrderId.Value, order.Status, order.RefundId);
    }
}
