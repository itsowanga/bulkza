using Application.Interfaces;
using Domain;
using Domain.ValueObjects;

namespace Application.BulkPools.CreatePool;

public class CreatePoolHandler (IBulkPoolRepository bulkPool, IProductRepository product, IUnitOfWork uow)

{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;
    private readonly IProductRepository _product = product;
    private readonly IUnitOfWork _uow = uow;

    public async Task<CreatePoolResult> Handle(CreatePoolCommand cmd, CancellationToken ct)
    {
        var product = await _product.GetByIdAsync(cmd.ProductId, ct) ?? throw new KeyNotFoundException("Product does not exist.");
        if (!product.IsActive) throw new InvalidOperationException("Product is not active.");

        BulkPool pool = new(PoolId.New(), cmd.ProductId, cmd.MinimumQuantity, cmd.MinimumMerchants, cmd.Deadline);
        pool.Join(cmd.MerchantId, cmd.MerchantQuantity);
        await _bulkPool.AddAsync(pool, ct);
        await _uow.SaveChangesAsync(ct);
        return new CreatePoolResult(pool.PoolId.Value, pool.Status, pool.CreatedAt);
    }
}