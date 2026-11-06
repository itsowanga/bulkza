using Application.Interfaces;
using Domain;

namespace Application.BulkPools.CancelPool;
public class CancelPoolHandler(IBulkPoolRepository bulkPool, IUnitOfWork uow)
{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;
    private readonly IUnitOfWork _uow = uow;

    public async Task<CancelPoolResult> Handle(CancelPoolCommand cmd, CancellationToken ct)
    {
        BulkPool pool = await _bulkPool.GetByIdAsync(cmd.PoolId, ct) ?? throw new KeyNotFoundException("Pool does not exist.");
        pool.Cancel();
        await _uow.SaveChangesAsync(ct);
        return new CancelPoolResult(pool.PoolId.Value, pool.Status);
    }
}