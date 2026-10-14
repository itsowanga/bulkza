using Application.Interfaces;
using Domain;

namespace Application.Orders.MarkAsDelivered;

public class MarkAsDeliveredHandler(IOrderRepository order, IUnitOfWork uow)
{
    private readonly IOrderRepository _order = order;
    private readonly IUnitOfWork _uow = uow;

    public async Task<MarkAsDeliveredResult> Handle(MarkAsDeliveredCommand cmd, CancellationToken ct)
    {
        Order order_ = await _order.GetOrderByIdAsync(cmd.OrderId, ct) ?? throw new KeyNotFoundException("Order is not found.");
        order_.MarkAsDelivered();
        _order.Update(order_);
        await _uow.SaveChangesAsync(ct);
        return new(order_.OrderId, order_.MerchantId, order_.OrderLines, order_.Status);
    }
}