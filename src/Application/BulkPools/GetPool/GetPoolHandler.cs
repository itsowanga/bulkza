using Application.Interfaces;
using Domain.ValueObjects;

namespace Application.BulkPools.GetPool;

public record GetPoolQuery(PoolId PoolId, MerchantId MerchantId);

public class GetPoolHandler(IBulkPoolRepository bulkPool)
{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;

    public async Task<BulkPoolDto> Handle(GetPoolQuery query, CancellationToken ct)
    {
        var pool = await _bulkPool.GetByIdAsync(query.PoolId, ct) ?? throw new KeyNotFoundException("Pool does not exist.");
        return BulkPoolDto.From(pool, query.MerchantId);
    }
}
