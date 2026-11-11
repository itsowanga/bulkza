using Application.Interfaces;
using Domain;
using Domain.Enums;

namespace Application.BulkPools.SearchPool.SearchByMerchant;

public class SearchByMerchantHandler(IBulkPoolRepository pool)
{
    private readonly IBulkPoolRepository _pool = pool;

    public async Task<PagedResult<BulkPoolDto>> Handle(SearchByMerchantQuery query, CancellationToken ct)
    {
        if (query.Page < 1) throw new ArgumentException("Page number cannot be less than 1.");
        if (query.PageSize < 1 || query.PageSize > 50) throw new ArgumentException("Page size must be between 1 and 50.");
        var (pools, totalPools) = await _pool.GetByMerchantIdAsync(query.MerchantId, query.Page, query.PageSize, ct);
        var dtos = pools.Select(pool => BulkPoolDto.From(pool, query.MerchantId)).ToList();
        
        return new(dtos, totalPools, query.Page, query.PageSize);

    }
}

