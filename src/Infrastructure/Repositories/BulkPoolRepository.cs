using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Infrastructure.Repositories;

public class BulkPoolRepository(BulkzaDbContext db) : IBulkPoolRepository
{
    private readonly BulkzaDbContext _db = db;

    public Task<BulkPool?> GetByIdAsync(PoolId id, CancellationToken ct = default) => _db.BulkPools.FirstOrDefaultAsync(pool => pool.PoolId == id, ct);
    public async Task<IReadOnlyList<BulkPool>> GetOpenByProductIdAsync(ProductId productId, CancellationToken ct = default) => await _db.BulkPools
                                                                                            .Where(pool => (pool.ProductId == productId) && (pool.Status == BulkPoolStatus.Open)).ToListAsync(ct);
    public async Task<IReadOnlyList<BulkPool>> GetByMerchantIdAsync(MerchantId merchantId, CancellationToken ct = default)
    {
        var pools = await _db.BulkPools
        .FromSqlRaw(@"
                      SELECT * FROM BulkPools
                      WHERE EXISTS (
                      SELECT 1 FROM OPENJSON(Participants)
                      WITH (MerchantId uniqueidentifier '$.MerchantId') AS p
                      WHERE p.MerchantId = {0}
                      )", merchantId.Value).ToListAsync(ct);
        return pools;
    }

    public async Task AddAsync(BulkPool pool, CancellationToken ct = default) => await _db.BulkPools.AddAsync(pool, ct);
    public void Update(BulkPool pool) => _db.BulkPools.Update(pool);
}


