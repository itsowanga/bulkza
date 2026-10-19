using Application.Interfaces;
using Domain;
using Domain.Enums;

namespace Application.BulkPools.SearchPool.SearchOpenByProductId;

public class SearchOpenByProductIdHandler(IBulkPoolRepository pool)
{
    private readonly IBulkPoolRepository _pool = pool;

    public async Task<PagedResult<BulkPoolDto>> Handle(SearchOpenByProductIdQuery query, CancellationToken ct)
    {
        var (pools, totalPools) = await _pool.GetOpenByProductIdAsync(query.ProductId, query.Page, query.PageSize, ct);
        var dtos = pools.Select(pool => 
        {
            var participant = pool.Participants.FirstOrDefault(p => p.MerchantId == query.MerchantId);
            var myQuantity = participant != default ? participant.Quantity.Value : 0;

            return new BulkPoolDto(pool.PoolId.Value, pool.ProductId.Value,
                                             pool.Status, pool.CurrentQuantity, myQuantity, 
                                             pool.Deadline);
        }).ToList();
        return new(dtos, totalPools, query.Page, query.PageSize);

    }
}

