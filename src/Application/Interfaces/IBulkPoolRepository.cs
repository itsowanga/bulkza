using Domain;
using Domain.ValueObjects;

namespace Application.Interfaces;
public interface IBulkPoolRepository
{
    Task<BulkPool?> GetByIdAsync(PoolId id, CancellationToken ct = default);
    Task<IReadOnlyList<BulkPool>> GetOpenByProductIdAsync(ProductId productId, CancellationToken ct = default);
    Task<IReadOnlyList<BulkPool>> GetByMerchantIdAsync(MerchantId merchantId, CancellationToken ct = default);
    Task AddAsync(BulkPool pool, CancellationToken ct = default);
    void Update(BulkPool pool);
}