using Application.Interfaces;
using Domain;

namespace Application.Orders.CreateOrder;

public class CreateOrderHandler(IOrderRepository order, IUnitOfWork uow)
{
    private readonly IOrderRepository _order = order;
    private readonly IUnitOfWork _uow = uow;

    public async Task<CreateOrderResult> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        Order order_ = new(cmd.OrderId, cmd.MerchantId, cmd.OrderLines);
        await _order.AddAsync(order_, ct);
        await _uow.SaveChangesAsync(ct);
        return new(order_.OrderId, order_.MerchantId, order_.OrderLines, order_.Status);
    }
}