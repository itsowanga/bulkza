using Application.BulkPools.ExpirePool;
using Application.BulkPools.FinalisePool;
using Application.Interfaces;
using Domain;

namespace Application.BulkPools.ExpirePool;
public class ExpirePoolHandler(IBulkPoolRepository bulkPool, IUnitOfWork uow)
{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;
    private readonly IUnitOfWork _uow = uow;

    public async Task<ExpirePoolResult> Handle(ExpirePoolCommand cmd, CancellationToken ct)
    {
        BulkPool pool = await _bulkPool.GetByIdAsync(cmd.PoolId, ct) ?? throw new KeyNotFoundException("Pool does not exist.");
        pool.Expire();
        await _uow.SaveChangesAsync(ct);
        return new ExpirePoolResult(pool.PoolId.Value, pool.Status);
    }
}