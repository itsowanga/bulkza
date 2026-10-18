using Domain;
using Domain.ValueObjects;

namespace Application.Interfaces;
public interface IBulkPoolRepository
{
    Task<BulkPool?> GetByIdAsync(PoolId id, CancellationToken ct = default);
    Task<(IReadOnlyList<BulkPool>, int)> GetOpenByProductIdAsync(ProductId productId, int page, int pageSize, CancellationToken ct = default);
    Task<(IReadOnlyList<BulkPool>, int)> GetByMerchantIdAsync(MerchantId merchantId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(BulkPool pool, CancellationToken ct = default);
    void Update(BulkPool pool);
}