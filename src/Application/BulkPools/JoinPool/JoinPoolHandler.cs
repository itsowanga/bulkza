using Application.Interfaces;
using Domain;

namespace Application.BulkPools.JoinPool;

public class JoinPoolHandler(IBulkPoolRepository bulkPool, IUnitOfWork uow)
{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;
    private readonly IUnitOfWork _uow = uow;

    public async Task<JoinPoolResult> Handle(JoinPoolCommand cmd, CancellationToken ct)
    {
        BulkPool pool = await _bulkPool.GetByIdAsync(cmd.PoolId, ct) ?? throw new KeyNotFoundException("Pool does not exist.");
        pool.Join(cmd.MerchantId, cmd.Quantity);
        await _uow.SaveChangesAsync(ct);
        return new JoinPoolResult(pool.Status, pool.CurrentQuantity);
    }
}