using Application.Interfaces;
using Domain;
using Domain.Events;

namespace Application.BulkPools.LeavePool;

public class LeavePoolHandler(IBulkPoolRepository bulkPool, IUnitOfWork uow)
{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;
    private readonly IUnitOfWork _uow = uow;

    public async Task<LeavePoolResult> Handle(LeavePoolCommand cmd, CancellationToken ct)
    {
        BulkPool pool = await _bulkPool.GetByIdAsync(cmd.PoolId, ct)  ?? throw new KeyNotFoundException("Pool does not exist.");
        pool.Leave(cmd.MerchantId);
        _bulkPool.Update(pool);
        await _uow.SaveChangesAsync(ct);
        return new LeavePoolResult(pool.Status, pool.CurrentQuantity);
    }

}