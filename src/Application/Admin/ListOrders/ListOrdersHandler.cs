using Application.Interfaces;
using Domain.Enums;

namespace Application.Admin.ListOrders;

public record ListOrdersQuery(OrderStatus? Status, int Page, int PageSize);

public class ListOrdersHandler(IOrderRepository order, IMerchantRepository merchant)
{
    private readonly IOrderRepository _order = order;
    private readonly IMerchantRepository _merchant = merchant;

    public async Task<PagedResult<AdminOrderDto>> Handle(ListOrdersQuery query, CancellationToken ct)
    {
        if (query.Page < 1) throw new ArgumentException("Page number cannot be less than 1.");
        if (query.PageSize < 1 || query.PageSize > 50) throw new ArgumentException("Page size must be between 1 and 50.");
        if (query.Status is not null && !Enum.IsDefined(query.Status.Value)) throw new ArgumentException("Status entered is invalid.");

        var (orders, total) = await _order.GetAllAsync(query.Status, query.Page, query.PageSize, ct);

        // One lookup for the whole page, not one per order.
        var names = await _merchant.GetNamesAsync(orders.Select(order => order.MerchantId).Distinct().ToList(), ct);
        var dtos = orders.Select(order => AdminOrderDto.From(order, names.GetValueOrDefault(order.MerchantId.Value, "Unknown merchant"))).ToList();
        return new(dtos, total, query.Page, query.PageSize);
    }
}
