using Application.Interfaces;
using Domain;

namespace Application.Orders.CancelOrder;

public class CancelOrderHandler(IOrderRepository order, IUnitOfWork uow)
{
    private readonly IOrderRepository _order = order;
    private readonly IUnitOfWork _uow = uow;

    public async Task<CancelOrderResult> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        Order order_ = await _order.GetOrderByIdAsync(cmd.OrderId, ct) ?? throw new KeyNotFoundException("Order is not found.");
        order_.Cancel();
        _order.Update(order_);
        await _uow.SaveChangesAsync(ct);
        return new(order_.OrderId, order_.MerchantId, order_.OrderLines, order_.Status);
    }
}