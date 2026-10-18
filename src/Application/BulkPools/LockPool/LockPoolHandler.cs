using Application.Interfaces;
using Domain;

namespace Application.BulkPools.LockPool;
public class LockPoolHandler(IBulkPoolRepository bulkPool, IUnitOfWork uow)
{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;
    private readonly IUnitOfWork _uow = uow;

    public async Task<LockPoolResult> Handle(LockPoolCommand cmd, CancellationToken ct)
    {
        BulkPool pool = await _bulkPool.GetByIdAsync(cmd.PoolId, ct) ?? throw new KeyNotFoundException("Pool does not exist.");
        pool.Lock();
        _bulkPool.Update(pool);
        await _uow.SaveChangesAsync(ct);
        return new LockPoolResult(pool.PoolId.Value, pool.Status);
    }
}