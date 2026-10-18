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
        var dtos = order_.OrderLines.Select(line => new OrderLineDto(
                                                    line.ProductId.Value,
                                                    line.Quantity.Value,
                                                    line.UnitPrice.Amount
        )).ToList();
        await _uow.SaveChangesAsync(ct);
        return new(order_.OrderId.Value, dtos, order_.Status);
    }
}