using Application.BulkPools;
using Application.Interfaces;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Admin.ListPools;

public record ListPoolsQuery(MerchantId Viewer, BulkPoolStatus? Status, int Page, int PageSize);

public class ListPoolsHandler(IBulkPoolRepository pool)
{
    private readonly IBulkPoolRepository _pool = pool;

    public async Task<PagedResult<BulkPoolDto>> Handle(ListPoolsQuery query, CancellationToken ct)
    {
        if (query.Page < 1) throw new ArgumentException("Page number cannot be less than 1.");
        if (query.PageSize < 1 || query.PageSize > 50) throw new ArgumentException("Page size must be between 1 and 50.");
        if (query.Status is not null && !Enum.IsDefined(query.Status.Value)) throw new ArgumentException("Status entered is invalid.");

        var (pools, total) = await _pool.GetAllAsync(query.Status, query.Page, query.PageSize, ct);
        var dtos = pools.Select(pool => BulkPoolDto.From(pool, query.Viewer)).ToList();
        return new(dtos, total, query.Page, query.PageSize);
    }
}
