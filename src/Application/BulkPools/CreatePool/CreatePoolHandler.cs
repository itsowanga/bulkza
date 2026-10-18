using Application.Interfaces;
using Domain;

namespace Application.BulkPools.CreatePool;

public class CreatePoolHandler (IBulkPoolRepository bulkPool, IUnitOfWork uow)

{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;
    private readonly IUnitOfWork _uow = uow;

    public async Task<CreatePoolResult> Handle(CreatePoolCommand cmd, CancellationToken ct)
    {
        BulkPool pool = new(cmd.PoolId, cmd.ProductId, cmd.MinimumQuantity, cmd.MinimumMerchants, cmd.Deadline);
        await _bulkPool.AddAsync(pool, ct);
        await _uow.SaveChangesAsync(ct);
        return new CreatePoolResult(pool.PoolId.Value, pool.Status, pool.CreatedAt);
    }
}