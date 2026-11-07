using Application.Interfaces;
using Domain.Enums;

namespace Application.Admin.GetSummary;

public class GetSummaryHandler(IBulkPoolRepository pool, IOrderRepository order)
{
    private readonly IBulkPoolRepository _pool = pool;
    private readonly IOrderRepository _order = order;

    public async Task<AdminSummaryDto> Handle(CancellationToken ct)
    {
        var pools = await _pool.CountByStatusAsync(ct);
        var orders = await _order.CountByStatusAsync(ct);

        return new(
            Enum.GetValues<BulkPoolStatus>().ToDictionary(status => status, status => pools.GetValueOrDefault(status)),
            Enum.GetValues<OrderStatus>().ToDictionary(status => status, status => orders.GetValueOrDefault(status)));
    }
}
