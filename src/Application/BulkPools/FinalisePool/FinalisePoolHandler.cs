using Application.BulkPools.FinalisePool;
using Application.Interfaces;
using Domain;

namespace Application.BulkPools.FinalisePool;
public class FinalisePoolHandler(IBulkPoolRepository bulkPool, IUnitOfWork uow)
{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;
    private readonly IUnitOfWork _uow = uow;

    public async Task<FinalisePoolResult> Handle(FinalisePoolCommand cmd, CancellationToken ct)
    {
        BulkPool pool = await _bulkPool.GetByIdAsync(cmd.PoolId, ct) ?? throw new KeyNotFoundException("Pool does not exist.");
        pool.Finalise();
        _bulkPool.Update(pool);
        await _uow.SaveChangesAsync(ct);
        return new FinalisePoolResult(pool.PoolId, pool.Status);
    }
}