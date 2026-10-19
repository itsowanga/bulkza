using Application.Interfaces;
using Domain;
using Domain.ValueObjects;

namespace Application.Orders.SearchOrders.FindAllOrderByMerchant;



public class SearchByMerchantHandler(IOrderRepository order)
{
    private readonly IOrderRepository _order = order;

    public async Task<PagedResult<OrderDto>> Handle(FindAllOrderByMerchantQuery query, CancellationToken ct)
    {
        var (orders, totalOrders) = await _order.GetByMerchantIdAsync(query.MerchantId, query.Page, query.PageSize, ct);

        var dtos = orders.Select(order => new OrderDto(order.OrderId.Value, order.MerchantId.Value,
                                    order.Status, order.TotalAmount.Amount, order.CreatedAt, order.RefundId,
                                    order.PaymentId)).ToList();
        return new(dtos, totalOrders, query.Page, query.PageSize);

    }
}

