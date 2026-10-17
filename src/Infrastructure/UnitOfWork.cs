using Application.Interfaces;

namespace Infrastructure;

public class UnitOfWork(BulkzaDbContext db) : IUnitOfWork
{
    private readonly BulkzaDbContext _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}