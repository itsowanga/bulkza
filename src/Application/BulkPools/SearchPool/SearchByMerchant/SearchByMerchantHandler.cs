using Application.Interfaces;
using Domain;
using Domain.Enums;

namespace Application.BulkPools.SearchPool.SearchByMerchant;

public class SearchByMerchantHandler(IBulkPoolRepository pool)
{
    private readonly IBulkPoolRepository _pool = pool;

    public async Task<PagedResult<BulkPoolDto>> Handle(SearchByMerchantQuery query, CancellationToken ct)
    {
        var (pools, totalPools) = await _pool.GetByMerchantIdAsync(query.MerchantId, query.Page, query.PageSize, ct);
        var dtos = pools.Select(pool => new BulkPoolDto(pool.PoolId.Value, pool.ProductId.Value,
                                             pool.Status, pool.CurrentQuantity, pool.MinimumQuantity.Value, 
                                             pool.Deadline, pool.CreatedAt)).ToList();
        
        return new(dtos, totalPools, query.Page, query.PageSize);

    }
}

